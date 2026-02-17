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
    /*
     PUT /products-v2
{
  "settings": {
    "number_of_shards": 3,
    "number_of_replicas": 1,
    "refresh_interval": "5s",
    "analysis": {
      "analyzer": {
        "product_name_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": [
            "lowercase",
            "stop",
            "snowball",
            "product_synonyms"
          ]
        },
        "autocomplete_analyzer": {
          "type": "custom",
          "tokenizer": "edge_ngram_tokenizer",
          "filter": ["lowercase"]
        }
      },
      "tokenizer": {
        "edge_ngram_tokenizer": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 20,
          "token_chars": ["letter", "digit"]
        }
      },
      "filter": {
        "product_synonyms": {
          "type": "synonym",
          "synonyms": [
            "laptop, notebook, computer",
            "phone, mobile, smartphone",
            "tv, television",
            "cheap, affordable, budget"
          ]
        },
        "custom_stop": {
          "type": "stop",
          "stopwords": ["the", "a", "an", "and", "or"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "name": {
        "type": "text",
        "analyzer": "product_name_analyzer",
        "search_analyzer": "product_name_analyzer",
        "fields": {
          "keyword": { "type": "keyword", "ignore_above": 256 },
          "autocomplete": {
            "type": "text",
            "analyzer": "autocomplete_analyzer",
            "search_analyzer": "standard"
          }
        }
      },
      "description": { 
        "type": "text", 
        "analyzer": "product_name_analyzer",
        "search_analyzer": "product_name_analyzer"
      },
      "price": { "type": "float" },
      "stock": { "type": "integer" },
      "category": {
        "type": "keyword",
        "fields": {
          "text": { "type": "text" }
        }
      },
      "tags": { "type": "keyword" },
      "createdDate": {
        "type": "date",
        "format": "strict_date_optional_time||epoch_millis"
      },
      "isActive": { "type": "boolean" },
      "specifications": {
        "type": "nested",
        "properties": {
          "brand": { "type": "keyword" },
          "model": { "type": "keyword" }
        }
      }
    }
  }
}
    */
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
                            .TokenChars(TokenChar.Letter, TokenChar.Digit) // Only letters and digits. ex: "laptop123" -> "l", "ap", "pt", "op", "123"
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
                        .SearchAnalyzer("product_name_analyzer")
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
                        .Analyzer("product_name_analyzer")
                        .SearchAnalyzer("product_name_analyzer")
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
    /*{
  "acknowledged": true, // Index creation acknowledged by cluster
  "shards_acknowledged": true, // All shards allocated successfully because of 3 shards and 1 replica
  //if shards_acknowledged is false it means that index creation is acknowledged but not all shards are allocated yet, which can lead to issues when indexing/searching until shards are ready
  "index": "products-v2" // Name of the created index
}*/

    /// <summary>
    /// Test analyzer with sample text
    /// Shows how text is tokenized and analyzed
    /// request example: standard, simple, whitespace, product_name_analyzer, autocomplete_analyzer
    /// </summary>
    /*
     POST /products-v2/_analyze
    {
      "analyzer": "autocomplete_analyzer",
      "text": "laptop"
    }
    
    GET /products/_search
    {
      "profile": true, // Enable profiling to see how query is executed and which analyzers are used
      "query": {
        "bool": {
          "must": [
            { "match": { "name": "laptop" } }
          ],
          "filter": [
            { "term": { "isActive": true } }
          ]
        }
      }
    }
     */
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
    ///
    /// GET /products/_mapping
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


    /// <summary>
    /// Reindex data from old index to new index
    /// Useful when you need to change mapping of existing fields
    /// example: reindex from "products" to "products-v2" after changing mapping in "products-v2"
    /// </summary>
    /*
     POST /_reindex
    {
      "source": {
        "index": "products"
      },
      "dest": {
        "index": "products-v2"
      }
    }
     */
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
    /*
       "took": 36,
      "timed_out": false,
      "total": 30,
      "updated": 0,
      "created": 30,
      "deleted": 0,
      "batches": 1, // Number of batches processed (depends on batch size and total documents)
      "version_conflicts": 0,
      "noops": 0,
      "retries": {
        "bulk": 0, // Number of bulk retries due to transient errors
        "search": 0 // Number of search retries (if source index is large and needs to scroll)
      },
      "throttled_millis": 0,
      "requests_per_second": -1,
      "throttled_until_millis": 0,
      "failures": []
    }
     */
    
    
    //compare products-v2 mapping with products mapping to see differences in field types, analyzers, etc.
     /*
    GET /products-v2,products/_mapping
     
     POST /products-v2/_analyze
    {
      "analyzer": "product_name_analyzer",
      "text": "MacBooks Pro 16 გზაზე მირბის კურდღელი"
    }
    
    GET /products-v2/_explain/2
    {
      "query": {
        "match": { "tags": "laptop" }
      }
    }

    POST /products-v2/_analyze
    {
      "field": "name",
      "text": "MacBookS Pro 16 გზაზე მირბის კურდღელი"
    }
     
     POST /products-v2/_analyze
    {
      "analyzer": "product_name_analyzer",
      "text": "Laptops"
    }
    
    response:
    # POST /products-v2/_analyze 200 OK
    {
      "tokens": [
        {
          "token": "laptop",
          "start_offset": 0,
          "end_offset": 7,
          "type": "<ALPHANUM>",
          "position": 0
        },
        {
          "token": "notebook",
          "start_offset": 0,
          "end_offset": 7,
          "type": "SYNONYM",
          "position": 0
        },
        {
          "token": "comput",
          "start_offset": 0,
          "end_offset": 7,
          "type": "SYNONYM",
          "position": 0
        }
      ]
    }
    # POST /products/_analyze 200 OK
    {
      "tokens": [
        {
          "token": "laptops",
          "start_offset": 0,
          "end_offset": 7,
          "type": "<ALPHANUM>",
          "position": 0
        }
      ]
    }
    
    compare:
    GET /products,products-v2/_search
    {
      "profile": true,
      "query": {
        "match": {
          "name": "laptop"
        }
      }
    }
    GET /products-v2/_search
    {
      "query": {
        "match": {
          "tags": {
            "query": "notebook",
            "analyzer": "product_name_analyzer"
          }
        }
      }
    }
     */
}

