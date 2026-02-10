using Nest;

namespace ElasticSearch.Core.Services;

/// <summary>
/// Static executor for Hangfire jobs - Hangfire can't serialize service instances
/// </summary>
public static class SyncJobExecutor
{
    private static string? _sqlConnectionString;
    private static string? _elasticsearchUri;

    public static void Initialize(string sqlConnectionString, string elasticsearchUri)
    {
        _sqlConnectionString = sqlConnectionString;
        _elasticsearchUri = elasticsearchUri;
    }
    
    public static async Task ExecuteFullSyncAsync()
    {
        if (string.IsNullOrEmpty(_sqlConnectionString) || string.IsNullOrEmpty(_elasticsearchUri))
        {
            throw new InvalidOperationException("SyncJobExecutor not initialized. Call Initialize() first.");
        }
        
        var settings = new ConnectionSettings(new Uri(_elasticsearchUri))
            .DefaultIndex("products")
            .DisableDirectStreaming();
        
        var elasticClient = new ElasticClient(settings);
        var syncService = new SqlToElasticsearchSyncService(elasticClient, _sqlConnectionString);
        
        await syncService.FullSyncAsync();
    }
    
    public static async Task ExecuteIncrementalSyncAsync()
    {
        if (string.IsNullOrEmpty(_sqlConnectionString) || string.IsNullOrEmpty(_elasticsearchUri))
        {
            throw new InvalidOperationException("SyncJobExecutor not initialized. Call Initialize() first.");
        }
        
        var settings = new ConnectionSettings(new Uri(_elasticsearchUri))
            .DefaultIndex("products")
            .DisableDirectStreaming(); // DisableDirectStreaming for better error diagnostics in case of sync failures
        
        var elasticClient = new ElasticClient(settings);
        var syncService = new SqlToElasticsearchSyncService(elasticClient, _sqlConnectionString);
        
        await syncService.IncrementalSyncAsync();
    }
}

