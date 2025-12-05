using Nest;
using ElasticSeach.Models;

namespace ElasticSeach.Services;

/// <summary>
/// 6. Data Search and Query DSL Basics
/// Demonstrates different types of queries
/// </summary>
public class SearchService
{
    private readonly ElasticClient _client;

    public SearchService(ElasticClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Check if index has documents
    /// </summary>
    public async Task<bool> HasDocumentsAsync(string indexName)
    {
        var countResponse = await _client.CountAsync<Product>(c => c
            .Index(indexName)
        );
        
        return countResponse.IsValid && countResponse.Count > 0;
    }

    /// <summary>
    /// Match All Query - returns all documents
    /// </summary>
    public async Task MatchAllQueryAsync()
    {
        Console.WriteLine("\n=== 6. Query DSL Basics ===");
        
        // Check if we have documents
        var hasData = await HasDocumentsAsync("products");
        if (!hasData)
        {
            Console.WriteLine("⚠ No documents found in 'products' index.");
            Console.WriteLine("  Please run option 3 (Indexing Documents) first!");
            return;
        }
        
        Console.WriteLine("6.1. Match All Query");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .MatchAll()
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Match Query - full-text search
    /// </summary>
    public async Task MatchQueryAsync()
    {
        Console.WriteLine("\n6.2. Match Query (full-text search)");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query("laptop dell")
                )
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Multi-Match Query - search across multiple fields
    /// </summary>
    public async Task MultiMatchQueryAsync()
    {
        Console.WriteLine("\n6.3. Multi-Match Query");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(f => f
                        .Field(ff => ff.Name, boost: 2.0)
                        .Field(ff => ff.Description)
                    )
                    .Query("smartphone camera")
                )
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Term Query - exact term matching
    /// </summary>
    public async Task TermQueryAsync()
    {
        Console.WriteLine("\n6.4. Term Query (exact match)");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Category)
                    .Value("Electronics")
                )
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Terms Query - match multiple exact terms
    /// </summary>
    public async Task TermsQueryAsync()
    {
        Console.WriteLine("\n6.5. Terms Query (multiple exact matches)");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.Tags)
                    .Terms("laptop", "smartphone", "tablet")
                )
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Range Query - numeric and date ranges
    /// </summary>
    public async Task RangeQueryAsync()
    {
        Console.WriteLine("\n6.6. Range Query");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Range(r => r
                    .Field(f => f.Price)
                    .GreaterThanOrEquals(500)
                    .LessThanOrEquals(1500)
                )
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Bool Query - combine multiple queries
    /// </summary>
    public async Task BoolQueryAsync()
    {
        Console.WriteLine("\n6.7. Bool Query (combining queries)");
        Console.WriteLine("Find Electronics with price < 1000 AND (Apple OR Samsung)");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Term(t => t.Field(f => f.Category).Value("Electronics"))
                    )
                    .Filter(f => f
                        .Range(r => r
                            .Field(ff => ff.Price)
                            .LessThan(1000)
                        )
                    )
                    .Should(
                        sh => sh.Match(m => m.Field(f => f.Name).Query("Apple")),
                        sh => sh.Match(m => m.Field(f => f.Name).Query("Samsung"))
                    )
                    .MinimumShouldMatch(1)
                )
            )
        );

        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Wildcard and Fuzzy Queries
    /// </summary>
    public async Task WildcardAndFuzzyQueriesAsync()
    {
        Console.WriteLine("\n6.8. Wildcard Query");

        var wildcardResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Wildcard(w => w
                    .Field(f => f.Name.Suffix("keyword"))
                    .Value("*phone*")
                )
            )
        );

        DisplayResults(wildcardResponse);

        Console.WriteLine("\n6.9. Fuzzy Query (typo tolerance)");

        var fuzzyResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Fuzzy(fz => fz
                    .Field(f => f.Name)
                    .Value("labtop") // typo: should be "laptop"
                    .Fuzziness(Fuzziness.Auto)
                )
            )
        );

        DisplayResults(fuzzyResponse);
    }

    /// <summary>
    /// Aggregations - analytics
    /// </summary>
    public async Task AggregationsAsync()
    {
        Console.WriteLine("\n6.10. Aggregations (Analytics)");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Size(0) // We only want aggregations, not documents
            .Aggregations(a => a
                .Terms("categories", t => t
                    .Field(f => f.Category)
                )
                .Average("average_price", av => av
                    .Field(f => f.Price)
                )
                .Max("max_price", m => m
                    .Field(f => f.Price)
                )
                .Min("min_price", m => m
                    .Field(f => f.Price)
                )
                .Sum("total_stock", su => su
                    .Field(f => f.Stock)
                )
            )
        );

        if (searchResponse.IsValid)
        {
            Console.WriteLine("\nAggregation Results:");
            
            var categories = searchResponse.Aggregations.Terms("categories");
            Console.WriteLine("\nProducts by Category:");
            foreach (var bucket in categories.Buckets)
            {
                Console.WriteLine($"  • {bucket.Key}: {bucket.DocCount} products");
            }

            var avgPrice = searchResponse.Aggregations.Average("average_price");
            Console.WriteLine($"\nAverage Price: ${avgPrice.Value:F2}");

            var maxPrice = searchResponse.Aggregations.Max("max_price");
            Console.WriteLine($"Max Price: ${maxPrice.Value:F2}");

            var minPrice = searchResponse.Aggregations.Min("min_price");
            Console.WriteLine($"Min Price: ${minPrice.Value:F2}");

            var totalStock = searchResponse.Aggregations.Sum("total_stock");
            Console.WriteLine($"Total Stock: {totalStock.Value}");
        }
    }

    /// <summary>
    /// Sorting and Pagination
    /// </summary>
    public async Task SortingAndPaginationAsync()
    {
        Console.WriteLine("\n6.11. Sorting and Pagination");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .From(0)
            .Size(3)
            .Sort(so => so
                .Descending(f => f.Price)
                .Ascending(f => f.Name.Suffix("keyword"))
            )
            .Query(q => q
                .MatchAll()
            )
        );

        Console.WriteLine($"Showing {searchResponse.Documents.Count} of {searchResponse.Total} results");
        DisplayResults(searchResponse);
    }

    /// <summary>
    /// Highlighting
    /// </summary>
    public async Task HighlightingAsync()
    {
        Console.WriteLine("\n6.12. Highlighting");

        var searchResponse = await _client.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Description)
                    .Query("processor camera")
                )
            )
            .Highlight(h => h
                .Fields(
                    f => f.Field(ff => ff.Description)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            )
        );

        if (searchResponse.IsValid)
        {
            Console.WriteLine($"\nFound {searchResponse.Total} results:");
            foreach (var hit in searchResponse.Hits)
            {
                Console.WriteLine($"\n• {hit.Source.Name}");
                if (hit.Highlight.ContainsKey("description"))
                {
                    Console.WriteLine($"  Highlight: {string.Join(" ... ", hit.Highlight["description"])}");
                }
            }
        }
    }

    /// <summary>
    /// Search Articles with text analysis
    /// </summary>
    public async Task SearchArticlesAsync()
    {
        Console.WriteLine("\n6.13. Searching Articles (Text Analysis)");

        var searchResponse = await _client.SearchAsync<Article>(s => s
            .Index("articles")
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sh => sh.Match(m => m
                            .Field(f => f.Title)
                            .Query("elasticsearch query")
                            .Boost(2.0)
                        ),
                        sh => sh.Match(m => m
                            .Field(f => f.Content)
                            .Query("elasticsearch query")
                        )
                    )
                )
            )
            .Highlight(h => h
                .Fields(
                    f => f.Field(ff => ff.Title),
                    f => f.Field(ff => ff.Content).FragmentSize(100)
                )
            )
        );

        if (searchResponse.IsValid)
        {
            Console.WriteLine($"\nFound {searchResponse.Total} articles:");
            foreach (var hit in searchResponse.Hits)
            {
                Console.WriteLine($"\n• {hit.Source.Title}");
                Console.WriteLine($"  Author: {hit.Source.Author}, Views: {hit.Source.ViewCount}");
                Console.WriteLine($"  Score: {hit.Score}");
                
                if (hit.Highlight.Any())
                {
                    foreach (var highlight in hit.Highlight)
                    {
                        Console.WriteLine($"  {highlight.Key}: {string.Join(" ... ", highlight.Value)}");
                    }
                }
            }
        }
    }

    private void DisplayResults(ISearchResponse<Product> searchResponse)
    {
        if (searchResponse.IsValid)
        {
            Console.WriteLine($"\nTotal hits: {searchResponse.Total}");
            Console.WriteLine($"Max score: {searchResponse.MaxScore}");
            Console.WriteLine("\nResults:");
            
            foreach (var hit in searchResponse.Hits)
            {
                Console.WriteLine($"\n• [{hit.Score:F2}] {hit.Source.Name}");
                Console.WriteLine($"  Price: ${hit.Source.Price}, Stock: {hit.Source.Stock}");
                Console.WriteLine($"  Category: {hit.Source.Category}");
            }
        }
        else
        {
            Console.WriteLine($"✗ Search error: {searchResponse.DebugInformation}");
        }
    }
}

