using Nest;
using ElasticSeach.Models;

namespace ElasticSeach.Services;

/// <summary>
/// 3. Data Types and Mappings
/// Demonstrates different Elasticsearch data types
/// </summary>
public class MappingService
{
    private readonly ElasticClient _client;

    public MappingService(ElasticClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Create Product index with proper mappings
    /// </summary>
    public async Task CreateProductMappingAsync()
    {
        Console.WriteLine("\n=== Creating Product Index Mapping ===");

        var indexName = "products";
        
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            await _client.Indices.DeleteAsync(indexName);
            Console.WriteLine("✓ Deleted existing products index");
        }

        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Map<Product>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t
                        .Name(n => n.Name)
                        .Fields(f => f
                            .Keyword(k => k.Name("keyword"))
                        )
                    )
                    .Text(t => t
                        .Name(n => n.Description)
                        .Analyzer("standard")
                    )
                    .Number(n => n
                        .Name(nn => nn.Price)
                        .Type(NumberType.ScaledFloat)
                        .ScalingFactor(100)
                    )
                    .Number(n => n
                        .Name(nn => nn.Stock)
                        .Type(NumberType.Integer)
                    )
                    .Keyword(k => k.Name(n => n.Category))
                    .Keyword(k => k.Name(n => n.Tags))
                    .Date(d => d.Name(n => n.CreatedDate))
                    .Boolean(b => b.Name(n => n.IsActive))
                    .Object<ProductSpecs>(o => o
                        .Name(n => n.Specifications)
                        .Properties(sp => sp
                            .Keyword(k => k.Name(s => s.Brand))
                            .Keyword(k => k.Name(s => s.Model))
                        )
                    )
                )
            )
        );

        if (createResponse.IsValid)
        {
            Console.WriteLine("✓ Products index created with proper mappings!");
            Console.WriteLine("  • name: text with keyword sub-field for sorting");
            Console.WriteLine("  • description: text field for full-text search");
            Console.WriteLine("  • price: scaled_float for precise decimals");
            Console.WriteLine("  • category: keyword for exact match and aggregations");
            Console.WriteLine("  • tags: keyword for filtering");
        }
        else
        {
            Console.WriteLine($"✗ Error: {createResponse.DebugInformation}");
        }
    }

    /// <summary>
    /// Create Articles index with proper mappings
    /// </summary>
    public async Task CreateArticleMappingAsync()
    {
        Console.WriteLine("\n=== Creating Article Index Mapping ===");

        var indexName = "articles";
        
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            await _client.Indices.DeleteAsync(indexName);
            Console.WriteLine("✓ Deleted existing articles index");
        }

        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Map<Article>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t
                        .Name(n => n.Title)
                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                    )
                    .Text(t => t
                        .Name(n => n.Content)
                        .Analyzer("standard")
                    )
                    .Keyword(k => k.Name(n => n.Author))
                    .Date(d => d.Name(n => n.PublishDate))
                    .Keyword(k => k.Name(n => n.Keywords))
                    .Number(n => n.Name(nn => nn.ViewCount).Type(NumberType.Integer))
                )
            )
        );

        if (createResponse.IsValid)
        {
            Console.WriteLine("✓ Articles index created with proper mappings!");
        }
        else
        {
            Console.WriteLine($"✗ Error: {createResponse.DebugInformation}");
        }
    }

    /// <summary>
    /// Demonstrate explicit mapping with various data types
    /// </summary>
    public async Task DemonstrateDataTypesAsync()
    {
        Console.WriteLine("\n=== 3. Data Types and Mappings ===");

        var indexName = "data-types-demo";
        
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            await _client.Indices.DeleteAsync(indexName);
        }

        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Map<dynamic>(m => m
                .Properties(p => p
                    // Text type - full-text search
                    .Text(t => t
                        .Name("full_text_field")
                        .Analyzer("standard")
                    )
                    // Keyword type - exact match, aggregations, sorting
                    .Keyword(k => k
                        .Name("exact_match_field")
                    )
                    // Date type
                    .Date(d => d
                        .Name("date_field")
                        .Format("yyyy-MM-dd||epoch_millis")
                    )
                    // Numeric types
                    .Number(n => n
                        .Name("integer_field")
                        .Type(NumberType.Integer)
                    )
                    .Number(n => n
                        .Name("long_field")
                        .Type(NumberType.Long)
                    )
                    .Number(n => n
                        .Name("double_field")
                        .Type(NumberType.Double)
                    )
                    .Number(n => n
                        .Name("float_field")
                        .Type(NumberType.Float)
                    )
                    // Boolean type
                    .Boolean(b => b
                        .Name("boolean_field")
                    )
                    // Object type (nested JSON)
                    .Object<dynamic>(o => o
                        .Name("object_field")
                        .Properties(op => op
                            .Text(t => t.Name("name"))
                            .Number(n => n.Name("value").Type(NumberType.Integer))
                        )
                    )
                    // Nested type (for independent querying)
                    .Nested<dynamic>(n => n
                        .Name("nested_field")
                        .Properties(np => np
                            .Keyword(k => k.Name("key"))
                            .Text(t => t.Name("value"))
                        )
                    )
                    // Geo-point type
                    .GeoPoint(g => g
                        .Name("location")
                    )
                    // IP type
                    .Ip(i => i
                        .Name("ip_address")
                    )
                )
            )
        );

        if (createResponse.IsValid)
        {
            Console.WriteLine("✓ Data types demonstration index created!");
            Console.WriteLine("\nSupported Data Types:");
            Console.WriteLine("  • Text - full-text search");
            Console.WriteLine("  • Keyword - exact matching, aggregations");
            Console.WriteLine("  • Date - date/time values");
            Console.WriteLine("  • Numeric - integer, long, double, float, scaled_float");
            Console.WriteLine("  • Boolean - true/false");
            Console.WriteLine("  • Object - JSON objects");
            Console.WriteLine("  • Nested - independent nested documents");
            Console.WriteLine("  • Geo-point - geographical coordinates");
            Console.WriteLine("  • IP - IP addresses");
        }
    }

    /// <summary>
    /// Show mapping for an index
    /// </summary>
    public async Task ShowMappingAsync(string indexName)
    {
        Console.WriteLine($"\n=== Mapping for {indexName} ===");

        var mappingResponse = await _client.Indices.GetMappingAsync<Product>(m => m.Index(indexName));
        
        if (mappingResponse.IsValid)
        {
            Console.WriteLine("✓ Mapping retrieved successfully");
            Console.WriteLine("Properties:");
            
            foreach (var index in mappingResponse.Indices)
            {
                foreach (var property in index.Value.Mappings.Properties)
                {
                    Console.WriteLine($"  • {property.Key.Name}");
                }
            }
        }
    }

    /// <summary>
    /// Update mapping - add new field
    /// </summary>
    public async Task AddFieldToMappingAsync(string indexName)
    {
        Console.WriteLine($"\n=== Adding new field to {indexName} mapping ===");

        var putMappingResponse = await _client.MapAsync<Product>(m => m
            .Index(indexName)
            .Properties(p => p
                .Keyword(k => k
                    .Name("newField")
                )
            )
        );

        if (putMappingResponse.IsValid)
        {
            Console.WriteLine("✓ New field added to mapping!");
        }
    }
}

