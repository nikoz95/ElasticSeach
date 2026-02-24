using Nest;
using ElasticSearch.Core.Models;

namespace ElasticSearch.Core.Services;

/// <summary>
/// Advanced search features: Bool queries, Fuzzy search, Wildcards, Highlighting, etc.
/// </summary>
public class AdvancedSearchService(ElasticClient elasticClient)
{
    /// <summary>
    /// Complex Bool Query: Must + Filter + Should
    /// Example: Search "laptop" in Laptops category under $2000 with boost for featured products
    /// </summary>
    /*
     GET /products/_search
    {
        "from": 0,
        "size": 20,
        "query": {
            "bool": {
                "must": [
                {
                    "multi_match": {
                        "query": "laptop",
                        "fields": [
                        "name^2.0",
                        "description",
                        "category"
                            ],
                        "type": "best_fields",
                        "fuzziness": "AUTO"
                    }
                }
                ],
                "filter": [
                {
                    "match": {
                        "category": "Laptops"
                    }
                },
                {
                    "range": {
                        "price": {
                            "lte": 2000
                        }
                    }
                },
                {
                    "term": {
                        "isActive": true
                    }
                }
                ],
                "should": [
                {
                    "term": {
                        "tags": {
                            "value": "featured", // Boost products tagged as "featured"
                            "boost": 1.5
                        }
                    }
                },
                {
                    "range": {
                        "stock": {
                            "gt": 0,
                            "boost": 1.2
                        }
                    }
                }
                ],
                "minimum_should_match": 0
            }
        },
        "sort": [
        { "_score": "desc" },
        { "createdDate": "desc" }
        ],
        "highlight": {
            "pre_tags": ["<mark>"],
            "post_tags": ["</mark>"],
            "fields": {
                "name": { "number_of_fragments": 0 },
                "description": { "fragment_size": 150, "number_of_fragments": 1 }
            }
        }
    }
    */
    public async Task<List<Product>> ComplexBoolSearchAsync(
        string query, 
        string? category = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => // Build a bool query with MUST, FILTER, and SHOULD clauses
                {
                    var boolQuery = b;

                    // MUST: Required match (full-text search)
                    if (!string.IsNullOrEmpty(query))
                    {
                        boolQuery = boolQuery.Must(m => m
                            .MultiMatch(mm => mm
                                .Query(query)
                                .Fields(f => f
                                    .Field(p => p.Name, boost: 2.0)  // Name more important than description
                                    .Field(p => p.Description)
                                    .Field(p => p.Category)
                                )
                                .Type(TextQueryType.BestFields) // Use best_fields to get the highest score from any field
                                .Fuzziness(Fuzziness.Auto) // Allow typos in the main query (e.g., "laptap" → "laptop"
                            )
                        );
                    }
                    
                    //BestFields is the default for multi_match, it calculates the score based on the best matching field.
                    //MostFields calculates the score based on the total number of matching fields.
                    // CrossFields treats multiple fields as a single field, useful for searching across related fields (e.g., first_name + last_name).
                    // Phrase and PhrasePrefix are for exact phrase matching, with optional prefix support for autocomplete.
                    // Fuzziness allows for typo tolerance, with Auto adjusting based on query length (1-2 chars = 0 edits, 3-5 chars = 1 edit, 5+ chars = 2 edits).

                    // FILTER: Exact match, no scoring (faster)
                    // Functions for dynamic filters based on optional parameters
                    // QueryContainerDescriptor and QueryContainer are used to build complex queries in a fluent way. 
                    // BoolQuery is alternative to BoolQueryDescriptor, it allows you to build a bool query with multiple clauses (must, filter, should, must_not) in a more flexible way.
                    var filters = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();
                    
                    if (!string.IsNullOrEmpty(category))
                    {
                        filters.Add(f => f.Match(m => m.Field(p => p.Category).Query(category)));
                    }

                    if (maxPrice.HasValue)
                    {
                        filters.Add(f => f.Range(r => r
                            .Field(p => p.Price)
                            .LessThanOrEquals((double)maxPrice.Value)
                        ));
                    }

                    // Only active products
                    filters.Add(f => f.Term(t => t.Field(p => p.IsActive).Value(true)));

                    if (filters.Count != 0)
                    {
                        boolQuery = boolQuery.Filter(filters.ToArray());
                    }

                    // SHOULD: Optional boost (doesn't exclude results)
                    boolQuery = boolQuery.Should(
                        sh => sh.Term(t => t.Field(p => p.Tags).Value("featured").Boost(1.5)),
                        sh => sh.Range(r => r.Field(p => p.Stock).GreaterThan(0).Boost(1.2))
                    ).MinimumShouldMatch(0);

                    return boolQuery;
                })
            )
            .Sort(so => so
                .Descending(SortSpecialField.Score)  // Relevance first
                .Descending(p => p.CreatedDate)      // Then newest
            )
            .Highlight(h => h
                .PreTags("<mark>")
                .PostTags("</mark>")
                .Fields(
                    f => f.Field(p => p.Name).NumberOfFragments(0),
                    f => f.Field(p => p.Description).FragmentSize(150).NumberOfFragments(1)
                )
            )
        );

        if (!response.IsValid)
        {
            return [];
        }

        // Add highlight info to products (for display)
        var products = response.Documents.ToList();
        foreach (var hit in response.Hits)
        {
            if (hit.Highlight == null || !hit.Highlight.Any()) continue;
            var product = products.FirstOrDefault(p => p.Id == hit.Id);
            if (product == null) continue;
            // Store highlights in Description for demo (in real app, return separate DTO)
            if (hit.Highlight.TryGetValue("description", out var value))
            {
                product.Description = string.Join(" ... ", value);
            }
        }

        return products;
    }
    /*
     {
          "took": 6, // Time taken to execute the search in milliseconds
          "timed_out": false,
          "_shards": { // Shard is a subset of the data, Elasticsearch distributes data across multiple shards for scalability and performance
            "total": 1, // Total number of shards that were queried
            "successful": 1, // Number of shards that successfully executed the search
            "skipped": 0, // Number of shards that were skipped (e.g., due to node failures or other issues)
            "failed": 0 // Number of shards that failed to execute the search (e.g., due to errors or timeouts)
          },
          "hits": {
            "total": {
              "value": 2,
              "relation": "eq" // "eq" means the total hits is an exact count, "gte" means the total hits is greater than or equal to the value (used when track_total_hits is false for performance reasons)
            },
            "max_score": null,
            "hits": [
              {
                "_index": "products",
                "_id": "29",
                "_score": 2.501821, // Relevance score calculated by Elasticsearch based on the query and the document's content. Higher score means more relevant to the query.
                "_source": {
                  "id": "29",
                  "name": "Lenovo ThinkPad X1 Carbon",
                  "description": "ბიზნეს ლეპტოპი Intel Core i7, 16GB RAM, მსუბუქი და გამძლე",
                  "price": 1899.99,
                  "stock": 18,
                  "category": "Laptops",
                  "tags": [
                    "lenovo",
                    "laptop",
                    "thinkpad",
                    "business",
                    "portable"
                  ],
                  "createdDate": "2026-02-14T13:23:56.3700000",
                  "isActive": true,
                  "specifications": {
                    "brand": "Lenovo",
                    "model": "ThinkPad X1 Carbon Gen 11"
                  }
                },
                "sort": [
                  2.501821, // The first value in the sort array corresponds to the relevance score (_score) of the document. This is used when sorting by relevance.
                  1771075436370 // The second value corresponds to the createdDate field, which is used for secondary sorting when multiple documents have the same relevance score. This value is the epoch time in milliseconds for the createdDate of the document (2026-02-14T13:23:56.370Z).
                ]
              },
            ]
          }
        }
     */

    /// <summary>
    /// Fuzzy Search - tolerates typos (e.g., "dall" → "dell")
    /// Fuzziness=Auto: 1-2 chars = 0 edits, 3-5 chars = 1 edit, 5+ chars = 2 edits
    /// </summary>
    /*
     GET /products/_search
    {
      "query": {
        "match": {
          "name": {
            "query": "dall",
            "fuzziness": "AUTO",
            "prefix_length": 0,
            "max_expansions": 50
          }
        }
      }
    }
     */
    public async Task<List<Product>> FuzzySearchAsync(string query, int maxEdits = 0)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(query)
                    .Fuzziness(Fuzziness.Auto)
                    .PrefixLength(0)  // First 0 chars must match exactly
                    .MaxExpansions(50) // max check 50 variations words, ex: ["laatap", "labtap", ..., "laptop", "laptap", ...]
                )
            )
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : [];
    }

    /// <summary>
    /// Prefix Search - starts with query ex:"mac" matches "macbook" because "mac" is a prefix
    /// Faster than wildcard, optimized for autocomplete
    /// </summary>
    /*
     GET /products/_search
    {
      "size": 10,
      "query": {
        "match_phrase_prefix": {
          "name": {
            "query": "mac"
          }
        }
      },
      "sort": [
        {
          "name.keyword": {
            "order": "asc"
          }
        }
      ]
    }
     */
    public async Task<List<Product>> PrefixSearchAsync(string prefix, int limit = 10)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Size(limit)
            .Query(q => q
                .MatchPhrasePrefix(m => m
                    .Field(f => f.Name)
                    .Query(prefix)
                )
            )
            .Sort(so => so.Ascending(p => p.Name.Suffix("keyword")))
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : [];
    }

    /// <summary>
    /// Paginated search with total count
    /// </summary>
    /*
     GET /products/_search
    {
      "from": 0,
      "size": 20,
      "query": {
        "multi_match": {
          "query": "dell",
          "fields": [
            "name",
            "description"
          ]
        }
      },
      "track_total_hits": true
    }
     */
    public async Task<(List<Product> Products, long Total)> PaginatedSearchAsync(
        string query, 
        int page = 1, 
        int pageSize = 20)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(m => m
                    .Query(query)
                    .Fields(f => f.Field(p => p.Name).Field(p => p.Description))
                )
            )
            .TrackTotalHits() // return total count
        );

        if (!response.IsValid || response.Documents == null)
        {
            return ([], 0);
        }

        return (response.Documents.ToList(), response.Total);
    }
    /*
     GET /products/_search
    {
      "query": {
        "script_score": {
          "query": {
            "multi_match": {
              "query": "laptop",
              "fields": [
                "name",
                "description"
              ]
            }
          },
          "script": {
            "source": "double boost = doc['stock'].size() != 0 && doc['stock'].value > 0 ? 1.2 : 1.0; return _score * boost;"
          }
        }
      }
    }
     */
    /// <summary>
    /// Search with Script Score: Adjust relevance based on dynamic logic
    /// Example: Boost score for products in stock
    /// </summary>
    public async Task<List<Product>> ScriptScoreSearchAsync(string query)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .ScriptScore(ss => ss
                    .Query(qq => qq
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(f => f.Field(p => p.Name).Field(p => p.Description))
                        )
                    )
                    .Script(scr => scr
                        .Source("double boost = doc['stock'].size() != 0 && doc['stock'].value > 0 ? 1.2 : 1.0; return _score * boost;")
                    )
                )
            )
        );

        return response.IsValid ? response.Documents.ToList() : [];
    }
    
    /*
     GET /products/_search
    {
      "runtime_mappings": {
        "is_expensive": {
          "type": "boolean",
          "script": {
            "source": "emit(doc['price'].value > params.threshold)",
            "params": {
              "threshold": 100.0
            }
          }
        }
      },
      "query": {
        "term": {
          "is_expensive": true
        }
      }
    }
     */
    /// <summary>
    /// Search with Runtime Fields: Dynamic fields calculated during query
    /// Example: 'is_expensive' boolean based on price threshold
    /// </summary>
    public async Task<List<Product>> RuntimeFieldSearchAsync(decimal priceThreshold)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .RuntimeFields(rm => rm
                .RuntimeField("is_expensive", FieldType.Boolean, rf => rf
                    .Script(scr => scr
                        .Source("emit(doc['price'].value > params.threshold)")
                        .Params(p => p.Add("threshold", (double)priceThreshold))
                    )
                )
            )
            .Query(q => q
                .Term(t => t.Field("is_expensive").Value(true))
            )
        );

        return response.IsValid ? response.Documents.ToList() : [];
    }

    /*
     GET /products/_search
    {
      "query": {
        "match": {
          "name": "iphone"
        }
      },
      "script_fields": {
        "price_with_vat": {
          "script": {
            "source": "doc['price'].value * params.vat",
            "params": {
              "vat": 1.18
            }
          }
        }
      },
      "_source": ["*"]
    }
     */
    /// <summary>
    /// Search with Script Fields: Return calculated values not in index
    /// Example: Calculate price with VAT
    /// </summary>
    public async Task<object> ScriptFieldsSearchAsync(string query, double vatRate = 1.18)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m.Field(f => f.Name).Query(query))
            )
            .ScriptFields(sf => sf
                .ScriptField("price_with_vat", scr => scr
                    .Source("doc['price'].value * params.vat")
                    .Params(p => p.Add("vat", vatRate))
                )
            )
            .Source(src => src.IncludeAll()) // Include source fields as well
        );

        if (!response.IsValid) return new List<object>();

        // For Script Fields, we need to extract from Fields instead of Source
        return response.Hits.Select(h => new
        {
            Product = h.Source,
            PriceWithVat = h.Fields.Value<double>("price_with_vat")
        }).ToList();
    }
}

