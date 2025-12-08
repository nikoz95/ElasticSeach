using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Nest;
using ElasticSearch.Core.Services;

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     ELASTICSEARCH BACKGROUND JOBS SERVICE                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Get connection strings
var sqlConnectionString = configuration.GetConnectionString("SqlServer") 
    ?? throw new Exception("SqlServer connection string not found");
var hangfireConnectionString = configuration.GetConnectionString("Hangfire") 
    ?? throw new Exception("Hangfire connection string not found");
var elasticsearchUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";

Console.WriteLine($"🔗 SQL Server: {sqlConnectionString.Split(';')[0]}");
Console.WriteLine($"🔍 Elasticsearch: {elasticsearchUri}");
Console.WriteLine();

// Configure Elasticsearch client
var settings = new ConnectionSettings(new Uri(elasticsearchUri))
    .DefaultIndex("products")
    .DisableDirectStreaming();

var elasticClient = new ElasticClient(settings);

// Test Elasticsearch connection
try
{
    var pingResponse = await elasticClient.PingAsync();
    if (pingResponse.IsValid)
    {
        Console.WriteLine("✅ Connected to Elasticsearch");
    }
    else
    {
        Console.WriteLine("⚠️  Elasticsearch connection failed (will retry)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Elasticsearch error: {ex.Message}");
}

Console.WriteLine();

// Configure Hangfire
GlobalConfiguration.Configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
        SchemaName = "HangfireJobs"
    });

Console.WriteLine("✅ Hangfire configured");
Console.WriteLine();

// Create sync service
var syncService = new SqlToElasticsearchSyncService(elasticClient, sqlConnectionString);

// Setup recurring jobs
Console.WriteLine("📅 Setting up recurring jobs:");

RecurringJob.AddOrUpdate(
    "incremental-sync",
    () => syncService.IncrementalSyncAsync(),
    configuration["BackgroundJobs:IncrementalSyncCron"] ?? "*/5 * * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
);
Console.WriteLine("  ✓ Incremental Sync: Every 5 minutes");

RecurringJob.AddOrUpdate(
    "full-sync-daily",
    () => syncService.FullSyncAsync(),
    configuration["BackgroundJobs:FullSyncDailyCron"] ?? "0 2 * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
);
Console.WriteLine("  ✓ Full Sync: Daily at 2:00 AM");

RecurringJob.AddOrUpdate(
    "full-sync-weekly",
    () => syncService.FullSyncAsync(),
    configuration["BackgroundJobs:FullSyncWeeklyCron"] ?? "0 3 * * 0",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
);
Console.WriteLine("  ✓ Full Sync: Weekly (Sunday 3:00 AM)");

Console.WriteLine();

// Start Hangfire server
using var server = new BackgroundJobServer(new BackgroundJobServerOptions
{
    WorkerCount = 2,
    ServerName = "ElasticSearchSyncServer"
});

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     🚀 BACKGROUND JOBS SERVER STARTED                       ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Worker Count: 2");
Console.WriteLine("Server Name: ElasticSearchSyncServer");
Console.WriteLine();
Console.WriteLine("📊 Jobs Status:");
Console.WriteLine("  • Incremental Sync: Running every 5 minutes");
Console.WriteLine("  • Full Sync (Daily): 2:00 AM");
Console.WriteLine("  • Full Sync (Weekly): Sunday 3:00 AM");
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to stop...");
Console.WriteLine();

// Trigger initial sync
Console.WriteLine("🔥 Triggering initial full sync...");
BackgroundJob.Enqueue(() => syncService.FullSyncAsync());

// Keep running
var exitEvent = new ManualResetEvent(false);
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    exitEvent.Set();
};

exitEvent.WaitOne();

Console.WriteLine();
Console.WriteLine("🛑 Shutting down...");

