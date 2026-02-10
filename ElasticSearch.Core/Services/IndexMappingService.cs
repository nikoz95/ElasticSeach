using Nest;
using ElasticSearch.Core.Models;

namespace ElasticSearch.Core.Services;

/// <summary>
/// Index creation with custom mappings, analyzers, and settings
/// Demonstrates: Multi-fields, Nested objects, Custom analyzers, Index templates
/// </summary>
public class IndexMappingService(ElasticClient elasticClient)
{
    /// <summary>
    /// Create index with explicit mappings and settings
    /// Shows: Multi-field mappings (text + keyword), nested objects, completion suggester
    /// </summary>
    public async Task<bool> CreateProductIndexWithMappingsAsync(string indexName = "products-v2")
    {
        var response = await elasticClient.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(3)           // Horizontal scaling and divided data across nodes for fast reads
                .NumberOfReplicas(1)         // each shard has 1 replica (backUp) for high availability
                .RefreshInterval("5s")       // How often new docs become searchable, because index not shown in real-time when writing
                .Analysis(a => a
                    // Custom Analyzer for product names
                    .Analyzers(an => an
                        .Custom("product_name_analyzer", ca => ca
                            .Tokenizer("standard") // Standard tokenizer, that is, splits text into words
                            .Filters(
                                "lowercase", // Lowercase filter
                                "stop", // Remove common words like "the", "a", etc.
                                "snowball", // Stemming filter (convert words to their root form). ex: "running" -> "run"
                                "product_synonyms" // Synonym filter (replace words with synonyms)
                                )
                        )
                        .Custom("autocomplete_analyzer", ca => ca
                            .Tokenizer("edge_ngram_tokenizer") // EdgeNGram tokenizer, that is, splits text into n-grams. ex: "laptop" -> "l", "ap", "pt"
                            .Filters("lowercase")
                        )
                    )
                    // Custom Tokenizers
                    .Tokenizers(t => t
                        .EdgeNGram("edge_ngram_tokenizer", ng => ng // EdgeNGram tokenizer with custom settings. ex: "laptop" -> "l", "ap", "pt"
                            .MinGram(2)
                            .MaxGram(20)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit) // Only letters and digits. ex: "laptop123"
                        )
                    )
                    // Token Filters
                    .TokenFilters(tf => tf
                        .Synonym("product_synonyms", sy => sy
                            .Synonyms(
                                "laptop, notebook, computer",
                                "phone, mobile, smartphone",
                                "tv, television",
                                "cheap, affordable, budget"
                            )
                        )
                        .Stop("custom_stop", st => st
                            .StopWords("the", "a", "an", "and", "or")
                        )
                    )
                )
            )
            .Map<Product>(m => m
                .Properties(p => p
                    // ID as keyword
                    .Keyword(k => k.Name(n => n.Id))
                    
                    // Name: Multi-field (text for search + keyword for sorting/aggregations)
                    .Text(t => t
                        .Name(n => n.Name)
                        .Analyzer("product_name_analyzer")
                        .SearchAnalyzer("standard")
                        .Fields(f => f
                            .Keyword(k => k
                                .Name("keyword")
                                .IgnoreAbove(256)  // Don't index strings longer than 256
                            )
                            .Text(tt => tt
                                .Name("autocomplete")
                                .Analyzer("autocomplete_analyzer")
                                .SearchAnalyzer("standard")
                            )
                        )
                    )
                    
                    // Description: Full-text search
                    .Text(t => t
                        .Name(n => n.Description)
                        .Analyzer("standard")
                    )
                    
                    // Price: Scaled float (efficient for money)
                    .Number(n => n
                        .Name(nn => nn.Price)
                        .Type(NumberType.Float)
                    )
                    
                    // Stock: Integer
                    .Number(n => n
                        .Name(nn => nn.Stock)
                        .Type(NumberType.Integer)
                    )
                    
                    // Category: Keyword (exact match, aggregations)
                    .Keyword(k => k
                        .Name(n => n.Category)
                        .Fields(f => f
                            .Text(t => t.Name("text"))  // Also searchable as text
                        )
                    )
                    
                    // Tags: Keyword array
                    .Keyword(k => k.Name(n => n.Tags))
                    
                    // CreatedDate: Date with format
                    .Date(d => d
                        .Name(n => n.CreatedDate)
                        .Format("strict_date_optional_time||epoch_millis")
                    )
                    
                    // IsActive: Boolean
                    .Boolean(b => b.Name(n => n.IsActive))
                    
                    // Specifications: Nested object (maintains object relationship)
                    .Nested<ProductSpecs>(n => n
                        .Name(nn => nn.Specifications)
                        .Properties(pp => pp
                            .Keyword(k => k.Name(s => s.Brand))
                            .Keyword(k => k.Name(s => s.Model))
                        )
                    )
                )
            )
        );

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Index '{indexName}' created successfully");
            return true;
        }

        Console.WriteLine($"❌ Failed to create index: {response.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Create Index Template - applies settings to all indices matching pattern
    /// Example: All indices like "products-2024-*" will use this template
    /// </summary>
    public async Task<bool> CreateProductIndexTemplateAsync()
    {
        var response = await elasticClient.Indices.PutTemplateAsync("products-template", tmpl => tmpl
            .IndexPatterns("products-*")  // Matches: products-2024, products-v2, etc.
            .Settings(s => s
                .NumberOfShards(2)
                .NumberOfReplicas(1)
                .RefreshInterval("5s")
            )
            .Map<Product>(m => m
                .Properties(p => p
                    .Text(t => t
                        .Name(n => n.Name)
                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                    )
                    .Keyword(k => k.Name(n => n.Category))
                    .Number(n => n.Name(nn => nn.Price).Type(NumberType.Float))
                )
            )
        );

        if (response.IsValid)
        {
            Console.WriteLine("✅ Index template created successfully");
            return true;
        }

        Console.WriteLine($"❌ Failed to create template: {response.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Test analyzer with sample text
    /// Shows how text is tokenized and analyzed
    /// </summary>
    public async Task<List<string>> TestAnalyzerAsync(string text, string analyzer = "standard")
    {
        var response = await elasticClient.Indices.AnalyzeAsync(a => a
            .Index("products-v2")
            .Text(text)
            .Analyzer(analyzer)
        );

        if (!response.IsValid)
        {
            Console.WriteLine($"❌ Analyzer test failed: {response.DebugInformation}");
            return [];
        }

        Console.WriteLine($"\n🔍 Analyzing: '{text}' with '{analyzer}'");
        Console.WriteLine("Tokens generated:");
        
        var tokens = new List<string>();
        foreach (var token in response.Tokens)
        {
            tokens.Add(token.Token);
            Console.WriteLine($"  - Token: '{token.Token}' | Position: {token.Position} | " +
                            $"Start: {token.StartOffset} | End: {token.EndOffset}");
        }

        return tokens;
    }

    /// <summary>
    /// Get mapping for an existing index
    /// Useful for debugging and understanding index structure
    /// </summary>
    public async Task<string> GetIndexMappingAsync(string indexName = "products")
    {
        var response = await elasticClient.Indices.GetMappingAsync<Product>(m => m
            .Index(indexName)
        );

        if (response.IsValid)
        {
            return response.DebugInformation;
        }

        Console.WriteLine($"❌ Failed to get mapping: {response.DebugInformation}");
        return string.Empty;
    }

    //not working
    /// <summary>
    /// Reindex data from old index to new index
    /// Useful when you need to change mapping of existing fields
    /// </summary>
    public async Task<bool> ReindexAsync(string sourceIndex, string destIndex)
    {
        var response = await elasticClient.ReindexOnServerAsync(r => r
            .Source(s => s.Index(sourceIndex))
            .Destination(d => d.Index(destIndex))
            .WaitForCompletion()
        );

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Reindexed {response.Total} documents from '{sourceIndex}' to '{destIndex}'");
            return true;
        }

        Console.WriteLine($"❌ Reindex failed: {response.DebugInformation}");
        return false;
    }

    
    /// <summary>
    /// Create index with alias
    /// Aliases allow zero-downtime reindexing
    /// </summary>
    public async Task<bool> CreateIndexWithAliasAsync(string indexName, string aliasName)
    {
        var response = await elasticClient.Indices.CreateAsync(indexName, c => c
            .Map<Product>(m => m.AutoMap())
            .Aliases(a => a
                .Alias(aliasName)
            )
        );

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Index '{indexName}' created with alias '{aliasName}'");
            return true;
        }

        Console.WriteLine($"❌ Failed: {response.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Demonstrate different data types in Elasticsearch
    /// </summary>
    public class ProductWithAllDataTypes
    {
        // String types
        public string TextField { get; set; } = string.Empty;        // Full-text search
        public string KeywordField { get; set; } = string.Empty;     // Exact match, sorting, aggregations
        
        // Numeric types
        public int IntegerField { get; set; }
        public long LongField { get; set; }
        public float FloatField { get; set; }
        public double DoubleField { get; set; }
        public decimal ScaledFloatField { get; set; }
        
        // Date types
        public DateTime DateField { get; set; }
        public DateTimeOffset DateTimeOffsetField { get; set; }
        
        // Boolean
        public bool BooleanField { get; set; }
        
        // Arrays
        public List<string> StringArray { get; set; } = new();
        public List<int> NumberArray { get; set; } = new();
        
        // Nested object (maintains parent-child relationship)
        public NestedObject? NestedField { get; set; }
        
        // Object (flattened, fields become independent)
        public ObjectType? ObjectField { get; set; }
        
        // Geo types
        public GeoLocation? Location { get; set; }
        
        // IP address
        public string IpAddress { get; set; } = string.Empty;
        
        // Binary (base64)
        public byte[]? BinaryData { get; set; }
    }

    public class NestedObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ObjectType
    {
        public string Property1 { get; set; } = string.Empty;
        public string Property2 { get; set; } = string.Empty;
    }

    /// <summary>
    /// Example of all data type mappings
    /// </summary>
    public async Task<bool> CreateIndexWithAllDataTypesAsync()
    {
        var response = await elasticClient.Indices.CreateAsync("demo-data-types", c => c
            .Map<ProductWithAllDataTypes>(m => m
                .Properties(p => p
                    .Text(t => t.Name(n => n.TextField))
                    .Keyword(k => k.Name(n => n.KeywordField))
                    
                    .Number(n => n.Name(nn => nn.IntegerField).Type(NumberType.Integer))
                    .Number(n => n.Name(nn => nn.LongField).Type(NumberType.Long))
                    .Number(n => n.Name(nn => nn.FloatField).Type(NumberType.Float))
                    .Number(n => n.Name(nn => nn.DoubleField).Type(NumberType.Double))
                    .Number(n => n.Name(nn => nn.ScaledFloatField).Type(NumberType.Float))
                    
                    .Date(d => d.Name(n => n.DateField))
                    .Date(d => d.Name(n => n.DateTimeOffsetField))
                    
                    .Boolean(b => b.Name(n => n.BooleanField))
                    
                    .Keyword(k => k.Name(n => n.StringArray))
                    .Number(n => n.Name(nn => nn.NumberArray).Type(NumberType.Integer))
                    
                    .Nested<NestedObject>(n => n.Name(nn => nn.NestedField))
                    .Object<ObjectType>(o => o.Name(nn => nn.ObjectField))
                    
                    .GeoPoint(g => g.Name(n => n.Location))
                    
                    .Ip(i => i.Name(n => n.IpAddress))
                    
                    .Binary(b => b.Name(n => n.BinaryData))
                )
            )
        );

        return response.IsValid;
    }
}

