using Nest;
using ElasticSeach.Models;

namespace ElasticSeach.Services;

/// <summary>
/// 1. Creating Index and Templates
/// Demonstrates index creation with settings and templates
/// </summary>
public class IndexManagementService
{
    private readonly ElasticClient _client;

    public IndexManagementService(ElasticClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Create product index with custom settings
    /// </summary>
    public async Task CreateProductIndexAsync()
    {
        var indexName = "products";
        
        Console.WriteLine($"\n=== 1. Creating Index: {indexName} ===");

        // Check if index exists
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            Console.WriteLine($"Index {indexName} already exists. Deleting...");
            await _client.Indices.DeleteAsync(indexName);
        }

        var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("product_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "asciifolding")
                        )
                    )
                )
            )
            .Map<Product>(m => m
                .AutoMap()
                .Properties(p => p
                    .Text(t => t
                        .Name(n => n.Name)
                        .Analyzer("product_analyzer")
                    )
                    .Text(t => t
                        .Name(n => n.Description)
                        .Analyzer("standard")
                    )
                    .Keyword(k => k
                        .Name(n => n.Category)
                    )
                    .Number(n => n
                        .Name(nn => nn.Price)
                        .Type(NumberType.ScaledFloat)
                        .ScalingFactor(100)
                    )
                )
            )
        );

        if (createIndexResponse.IsValid)
        {
            Console.WriteLine($"✓ Index {indexName} created successfully!");
        }
        else
        {
            Console.WriteLine($"✗ Error creating index: {createIndexResponse.DebugInformation}");
        }
    }

    /// <summary>
    /// Create article index with text analysis settings
    /// </summary>
    public async Task CreateArticleIndexAsync()
    {
        var indexName = "articles";
        
        Console.WriteLine($"\n=== Creating Index: {indexName} (with text analysis) ===");

        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            await _client.Indices.DeleteAsync(indexName);
        }

        var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("english_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "english_stop", "english_stemmer")
                        )
                    )
                    .TokenFilters(tf => tf
                        .Stop("english_stop", st => st
                            .StopWords("_english_")
                        )
                        .Stemmer("english_stemmer", st => st
                            .Language("english")
                        )
                    )
                )
            )
            .Map<Article>(m => m
                .AutoMap()
                .Properties(p => p
                    .Text(t => t
                        .Name(n => n.Title)
                        .Analyzer("english_analyzer")
                        .Fields(f => f
                            .Keyword(k => k.Name("keyword"))
                        )
                    )
                    .Text(t => t
                        .Name(n => n.Content)
                        .Analyzer("english_analyzer")
                    )
                )
            )
        );

        if (createIndexResponse.IsValid)
        {
            Console.WriteLine($"✓ Index {indexName} created successfully with text analysis!");
        }
    }

    /// <summary>
    /// Create Index Template for multiple indices
    /// </summary>
    public async Task CreateIndexTemplateAsync()
    {
        Console.WriteLine("\n=== 2. Creating Index Template ===");

        var templateName = "logs_template";
        
        var putTemplateResponse = await _client.Indices.PutTemplateAsync(templateName, t => t
            .IndexPatterns("logs-*")
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
            )
            .Map<dynamic>(m => m
                .Properties(p => p
                    .Date(d => d.Name("timestamp"))
                    .Text(txt => txt.Name("message"))
                    .Keyword(k => k.Name("level"))
                    .Keyword(k => k.Name("service"))
                )
            )
        );

        if (putTemplateResponse.IsValid)
        {
            Console.WriteLine($"✓ Template {templateName} created successfully!");
            Console.WriteLine("  Pattern: logs-*");
            Console.WriteLine("  This template will apply to any index starting with 'logs-'");
        }
    }

    /// <summary>
    /// Get and display index information
    /// </summary>
    public async Task DisplayIndexInfoAsync(string indexName)
    {
        Console.WriteLine($"\n=== Index Information: {indexName} ===");

        var getIndexResponse = await _client.Indices.GetAsync(indexName);
        
        if (getIndexResponse.IsValid)
        {
            var index = getIndexResponse.Indices[indexName];
            Console.WriteLine($"Settings:");
            Console.WriteLine($"  - Shards: {index.Settings.NumberOfShards}");
            Console.WriteLine($"  - Replicas: {index.Settings.NumberOfReplicas}");
            Console.WriteLine($"✓ Mappings configured with {index.Mappings.Properties.Count} properties");
        }
    }
}

