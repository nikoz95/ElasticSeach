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

    /// <summary>
    /// Fuzzy Search - tolerates typos (e.g., "laptap" → "laptop")
    /// Fuzziness=Auto: 1-2 chars = 0 edits, 3-5 chars = 1 edit, 5+ chars = 2 edits
    /// </summary>
    public async Task<List<Product>> FuzzySearchAsync(string query, int maxEdits = 2)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(query)
                    .Fuzziness(Fuzziness.Auto)
                    .PrefixLength(2)  // First 2 chars must match exactly
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
}

