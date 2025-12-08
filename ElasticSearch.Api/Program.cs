using Nest;
using ElasticSearch.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Elasticsearch API", 
        Version = "v1",
        Description = "API for searching products in Elasticsearch"
    });
});

// Configure Elasticsearch
var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(elasticsearchUri))
    .DefaultIndex("products")
    .DisableDirectStreaming()
    .PrettyJson();

var elasticClient = new ElasticClient(settings);
builder.Services.AddSingleton(elasticClient);

// Get SQL Server connection string
var sqlConnectionString = builder.Configuration.GetConnectionString("SqlServer") 
    ?? throw new Exception("SqlServer connection string not found");

// Register services
builder.Services.AddScoped<ProductSearchService>();
builder.Services.AddScoped<AdvancedSearchService>();
builder.Services.AddScoped<IndexMappingService>();
builder.Services.AddScoped<DatabaseSeederService>(_ => new DatabaseSeederService(sqlConnectionString));

var app = builder.Build();

// Initialize database with test data on startup
Console.WriteLine("🚀 Initializing application...");
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.InitializeDatabaseAsync();
}
Console.WriteLine();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elasticsearch API V1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Elasticsearch API Started");
Console.WriteLine($"📍 Swagger UI: http://localhost:5000");
Console.WriteLine($"🔍 Elasticsearch: {elasticsearchUri}");

app.Run();

