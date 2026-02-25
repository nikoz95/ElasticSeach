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
    /// Create an Index Template
    /// Index templates allow you to define settings and mappings that will be automatically applied to new indices
    /// that match a specific name pattern (e.g., "logs-*" or "products-*").
    /// </summary>
    /*
     PUT /_index_template/products_template
    {
      "index_patterns": ["products-*"],
      "template": {
        "settings": {
          "number_of_shards": 1,
          "number_of_replicas": 1,
          "analysis": {
            "normalizer": {
              "lowercase_normalizer": {
                "type": "custom",
                "filter": ["lowercase", "asciifolding"]
              }
            }
          }
        },
        "mappings": {
          "properties": {
            "category": {
              "type": "keyword",
              "normalizer": "lowercase_normalizer"
            }
          }
        }
      }
    }
    */
    
    
    /*
     POST products-electronics/_doc/1
    {
      "category": "Laptops"
    }
     */
    public async Task<bool> CreateProductIndexTemplateAsync(string templateName = "products_template")
    {
        var response = await elasticClient.Indices.PutTemplateAsync(templateName, t => t
            .IndexPatterns("products-*")
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(1)
                .Analysis(a => a
                    .Normalizers(n => n
                        .Custom("lowercase_normalizer", cn => cn
                            .Filters("lowercase", "asciifolding")
                        )
                    )
                )
            )
            .Map<Product>(m => m
                .Properties(p => p
                    .Keyword(k => k
                        .Name(n => n.Category)
                        .Normalizer("lowercase_normalizer")
                    )
                )
            )
        );

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Index Template '{templateName}' created successfully");
            return true;
        }

        Console.WriteLine($"❌ Failed to create Index Template: {response.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Create index with explicit mappings and settings
    /// Shows: Multi-field mappings (text + keyword), nested objects, completion suggester, and NORMALIZERS
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
    
    POST /products-v2/_analyze
    {
      "analyzer": "product_name_analyzer",
      "text": "The Smart Laptops"
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
                    // Custom Normalizer for keyword fields
                    .Normalizers(n => n
                        .Custom("lowercase_normalizer", cn => cn
                            .Filters("lowercase", "asciifolding")
                        )
                    )
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
                        .Normalizer("lowercase_normalizer") // Apply normalizer for case-insensitive matching
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

    /// <summary>
    /// Update Alias: Switch alias from old index to new index (Zero-Downtime)
    /// </summary>
    /*
     POST /_aliases
    {
      "actions": [
        { "remove": { "index": "products-v1", "alias": "products_alias" } },
        { "add":    { "index": "products-v2", "alias": "products_alias" } }
      ]
    }
    */
    public async Task<bool> UpdateAliasAsync(string aliasName, string oldIndex, string newIndex)
    {
        var response = await elasticClient.Indices.BulkAliasAsync(new BulkAliasRequest
        {
            Actions = new List<IAliasAction>
            {
                new AliasRemoveAction { Remove = new AliasRemoveOperation { Index = oldIndex, Alias = aliasName } },
                new AliasAddAction { Add = new AliasAddOperation { Index = newIndex, Alias = aliasName } }
            }
        });

        if (response.IsValid)
        {
            Console.WriteLine("✅ Alias switched successfully");
            return true;
        }

        Console.WriteLine($"❌ Alias update failed: {response.DebugInformation}");
        return false;
    }

    /*
     POST /_aliases
    {
      "actions": [
        {
          "add": {
            "index": "products",
            "alias": "products_alias"
          }
        }
      ]
}
     */
    /// <summary>
    /// Add Alias to an index
    /// </summary>
    public async Task<bool> AddAliasAsync(string indexName, string aliasName)
    {
        var response = await elasticClient.Indices.PutAliasAsync(indexName, aliasName);

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Alias '{aliasName}' added to index '{indexName}'");
            return true;
        }

        Console.WriteLine($"❌ Failed to add alias: {response.DebugInformation}");
        return false;
    }
    
    /// <summary>
    /// Get indices associated with an alias
    /// </summary>
    /*
    GET /_alias/products_alias
    */
    public async Task<List<string>> GetIndicesForAliasAsync(string aliasName)
    {
        var response = await elasticClient.Indices.GetAliasAsync(aliasName);

        if (response.IsValid)
        {
            return response.Indices.Keys.Select(k => k.Name).ToList();
        }

        return new List<string>();
    }

    /// <summary>
    /// Shrink Index: Reduce the number of primary shards in an index
    /// The index must be marked as read-only before shrinking
    /// </summary>
    /*
    1. Mark read-only:
    PUT /my_index/_settings
    {
      "settings": {
        "index.blocks.write": true // Prevents any write operations to the index, ensuring data consistency during the shrink process
      }
    }
    
    2. Shrink:
    POST /my_index/_shrink/my_index_shrunk
    {
      "settings": {
        "index.number_of_shards": 1,
        "index.number_of_replicas": 1
      }
    }
    */
    public async Task<bool> ShrinkIndexAsync(string sourceIndex, string targetIndex, int targetShards = 1)
    {
        // 1. Mark source index as read-only
        var settingsResponse = await elasticClient.Indices.UpdateSettingsAsync(sourceIndex, s => s
            .IndexSettings(isettings => isettings
                .BlocksWrite()
            )
        );

        if (!settingsResponse.IsValid)
        {
            Console.WriteLine($"❌ Failed to mark index as read-only: {settingsResponse.DebugInformation}");
            return false;
        }

        // 2. Perform shrink operation
        var shrinkResponse = await elasticClient.Indices.ShrinkAsync(sourceIndex, targetIndex, s => s
            .Settings(st => st
                .NumberOfShards(targetShards)
                .NumberOfReplicas(1)
            )
        );

        if (shrinkResponse.IsValid)
        {
            Console.WriteLine($"✅ Index '{sourceIndex}' shrunk to '{targetIndex}' with {targetShards} shards");
            return true;
        }

        Console.WriteLine($"❌ Shrink failed: {shrinkResponse.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Split Index: Increase the number of primary shards
    /// The index must be marked as read-only before splitting
    /// </summary>
    /*
    POST /products/_split/my_index_split
    {
      "settings": {
        "index.number_of_shards": 6
      }
    }
    */
    public async Task<bool> SplitIndexAsync(string sourceIndex, string targetIndex, int targetShards = 6)
    {
        // 1. Mark source index as read-only
        await elasticClient.Indices.UpdateSettingsAsync(sourceIndex, s => s
            .IndexSettings(isettings => isettings.BlocksWrite())
        );

        // 2. Perform split
        var splitResponse = await elasticClient.Indices.SplitAsync(sourceIndex, targetIndex, s => s
            .Settings(st => st
                .NumberOfShards(targetShards)
            )
        );

        if (splitResponse.IsValid)
        {
            Console.WriteLine($"✅ Index '{sourceIndex}' split to '{targetIndex}' with {targetShards} shards");
            return true;
        }

        Console.WriteLine($"❌ Split failed: {splitResponse.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Clone Index: Create an exact copy of an index
    /// The index must be marked as read-only before cloning
    /// </summary>
    /*
    1. Mark read-only:
    PUT /products/_settings
    {
      "settings": {
        "index.blocks.write": true
      }
    }

    2. Clone:
    POST /products/_clone/products-v2
    */
    public async Task<bool> CloneIndexAsync(string sourceIndex, string targetIndex)
    {
        // 1. Mark source index as read-only
        await elasticClient.Indices.UpdateSettingsAsync(sourceIndex, s => s
            .IndexSettings(isettings => isettings.BlocksWrite())
        );

        // 2. Perform clone
        var cloneResponse = await elasticClient.Indices.CloneAsync(sourceIndex, targetIndex);

        if (cloneResponse.IsValid)
        {
            Console.WriteLine($"✅ Index '{sourceIndex}' cloned to '{targetIndex}'");
            return true;
        }

        Console.WriteLine($"❌ Clone failed: {cloneResponse.DebugInformation}");
        return false;
    }

    /// <summary>
    /// Force Merge: Optimize index segments for better search performance
    /// Use only on indices that are no longer being updated
    /// </summary>
    /*
    POST /my_index/_forcemerge?max_num_segments=1
    */
    public async Task<bool> ForceMergeAsync(string indexName, int maxSegments = 1)
    {
        var response = await elasticClient.Indices.ForceMergeAsync(indexName, f => f
            .MaxNumSegments(maxSegments)
        );

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Force merge completed for index '{indexName}'");
            return true;
        }

        Console.WriteLine($"❌ Force merge failed: {response.DebugInformation}");
        return false;
    }
    /// <summary>
    /// Update by Query with Script: Bulk update documents matching a query
    /// Example: Apply a 10% discount to all products in a category
    /// </summary>
    /*
    POST /products/_update_by_query
    {
      "query": {
        "term": {
          "category.keyword": "Electronics"
        }
      },
      "script": {
        "source": "ctx._source.price = ctx._source.price * (1.0 - params.discount)",
        "params": {
          "discount": 0.1
        }
      }
    }
    */
    public async Task<bool> BulkUpdateWithScriptAsync(string category, double discount)
    {
        var response = await elasticClient.UpdateByQueryAsync<Product>(u => u
            .Index("products")
            .Query(q => q
                .Term(t => t.Field(f => f.Category.Suffix("keyword")).Value(category))
            )
            .Script(scr => scr
                .Source("ctx._source.price = ctx._source.price * (1.0 - params.discount)")
                .Params(p => p.Add("discount", discount))
            )
        );

        if (response.IsValid)
        {
            Console.WriteLine($"✅ Bulk update finished. Updated: {response.Updated}");
            return true;
        }

        Console.WriteLine($"❌ Bulk update failed: {response.DebugInformation}");
        return false;
    }
}

