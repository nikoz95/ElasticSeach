using Nest;
using ElasticSearch.Core.Models;

namespace ElasticSearch.Core.Services;

/// <summary>
/// Advanced search features: Bool queries, Fuzzy search, Wildcards, Highlighting, etc.
/// </summary>
public class AdvancedSearchService
{
    private readonly ElasticClient _elasticClient;

    public AdvancedSearchService(ElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    /// <summary>
    /// Complex Bool Query: Must + Filter + Should
    /// Example: Search "laptop" in Electronics category under $2000 with boost for featured products
    /// </summary>
    public async Task<List<Product>> ComplexBoolSearchAsync(
        string query, 
        string? category = null, 
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b =>
                {
                    var boolQuery = b;

                    // MUST: Required match (full-text search)
                    if (!string.IsNullOrEmpty(query))
                    {
                        boolQuery = boolQuery.Must(m => m
                            .MultiMatch(mm => mm
                                .Query(query)
                                .Fields(f => f
                                    .Field(p => p.Name, boost: 2.0)  // Name more important
                                    .Field(p => p.Description)
                                    .Field(p => p.Category)
                                )
                                .Type(TextQueryType.BestFields)
                                .Fuzziness(Fuzziness.Auto)
                            )
                        );
                    }

                    // FILTER: Exact match, no scoring (faster)
                    var filters = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();
                    
                    if (!string.IsNullOrEmpty(category))
                    {
                        filters.Add(f => f.Term(t => t.Field(p => p.Category).Value(category)));
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

                    if (filters.Any())
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
            return new List<Product>();
        }

        // Add highlight info to products (for display)
        var products = response.Documents.ToList();
        foreach (var hit in response.Hits)
        {
            if (hit.Highlight != null && hit.Highlight.Any())
            {
                var product = products.FirstOrDefault(p => p.Id == hit.Id);
                if (product != null)
                {
                    // Store highlights in Description for demo (in real app, return separate DTO)
                    if (hit.Highlight.ContainsKey("description"))
                    {
                        product.Description = string.Join(" ... ", hit.Highlight["description"]);
                    }
                }
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
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(query)
                    .Fuzziness(Fuzziness.Auto)
                    .PrefixLength(2)  // First 2 chars must match exactly
                    .MaxExpansions(50)
                )
            )
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Wildcard Search - pattern matching (* = any chars, ? = single char)
    /// Example: "lap*" matches "laptop", "lapel", etc.
    /// WARNING: Slow on large datasets, use carefully!
    /// </summary>
    public async Task<List<Product>> WildcardSearchAsync(string pattern)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Wildcard(w => w
                    .Field(f => f.Name.Suffix("keyword"))  // Use keyword field for exact patterns
                    .Value($"*{pattern}*")
                    .CaseInsensitive(true)
                )
            )
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Prefix Search - starts with query
    /// Faster than wildcard, optimized for autocomplete
    /// </summary>
    public async Task<List<Product>> PrefixSearchAsync(string prefix, int limit = 10)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Size(limit)
            .Query(q => q
                .Prefix(p => p
                    .Field(f => f.Name.Suffix("keyword"))
                    .Value(prefix)
                )
            )
            .Sort(so => so.Ascending(p => p.Name.Suffix("keyword")))
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Regexp Search - regular expressions
    /// Example: "lap[a-z]{3}" matches "laptop"
    /// </summary>
    public async Task<List<Product>> RegexpSearchAsync(string pattern)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Regexp(r => r
                    .Field(f => f.Name.Suffix("keyword"))
                    .Value(pattern)
                )
            )
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Search products that have (or don't have) a specific field
    /// Example: Find products with/without specifications
    /// </summary>
    public async Task<List<Product>> ExistsQueryAsync(string fieldName, bool mustExist = true)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q =>
            {
                var existsQuery = q.Exists(e => e.Field(fieldName));
                return mustExist 
                    ? existsQuery 
                    : q.Bool(b => b.MustNot(existsQuery));
            })
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Multi-field search with different analyzers
    /// Searches both exact (keyword) and analyzed (text) versions
    /// </summary>
    public async Task<List<Product>> MultiFieldSearchAsync(string query)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Bool(b => b
                    .Should(
                        // Exact match on keyword field (higher boost)
                        sh => sh.Term(t => t
                            .Field(f => f.Name.Suffix("keyword"))
                            .Value(query)
                            .Boost(2.0)
                        ),
                        // Analyzed match on text field
                        sh => sh.Match(m => m
                            .Field(f => f.Name)
                            .Query(query)
                            .Boost(1.0)
                        )
                    )
                )
            )
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Function Score Query - Custom scoring based on business logic
    /// Example: Boost popular products or recently added items
    /// </summary>
    public async Task<List<Product>> FunctionScoreSearchAsync(string query)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .FunctionScore(fs => fs
                    .Query(fq => fq
                        .MultiMatch(m => m
                            .Query(query)
                            .Fields(f => f.Field(p => p.Name).Field(p => p.Description))
                        )
                    )
                    .Functions(funcs => funcs
                        // Boost products in stock
                        .Weight(w => w
                            .Filter(fi => fi.Range(r => r.Field(p => p.Stock).GreaterThan(0)))
                            .Weight(1.5)
                        )
                        // Boost cheaper products
                        .FieldValueFactor(fv => fv
                            .Field(p => p.Price)
                            .Modifier(FieldValueFactorModifier.Reciprocal)
                            .Factor(0.1)
                        )
                    )
                    .ScoreMode(FunctionScoreMode.Multiply)
                    .BoostMode(FunctionBoostMode.Multiply)
                )
            )
        );

        return response.IsValid && response.Documents != null 
            ? response.Documents.ToList() 
            : new List<Product>();
    }

    /// <summary>
    /// Get search suggestions (did you mean?)
    /// Uses term suggester for typo corrections
    /// </summary>
    public async Task<List<string>> GetSuggestionsAsync(string query)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Size(0)
            .Suggest(sg => sg
                .Term("name-suggestions", t => t
                    .Field(f => f.Name)
                    .Text(query)
                    .Size(5)
                )
            )
        );

        if (!response.IsValid || response.Suggest == null)
        {
            return new List<string>();
        }

        var suggestions = new List<string>();
        foreach (var suggestion in response.Suggest.Values)
        {
            foreach (var option in suggestion.SelectMany(s => s.Options))
            {
                suggestions.Add(option.Text);
            }
        }

        return suggestions.Distinct().ToList();
    }

    /// <summary>
    /// Paginated search with total count
    /// </summary>
    public async Task<(List<Product> Products, long Total)> PaginatedSearchAsync(
        string query, 
        int page = 1, 
        int pageSize = 20)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(m => m
                    .Query(query)
                    .Fields(f => f.Field(p => p.Name).Field(p => p.Description))
                )
            )
            .TrackTotalHits(true)
        );

        if (!response.IsValid || response.Documents == null)
        {
            return (new List<Product>(), 0);
        }

        return (response.Documents.ToList(), response.Total);
    }
}

