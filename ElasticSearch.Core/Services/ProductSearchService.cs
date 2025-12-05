using Nest;
using ElasticSearch.Core.Models;

namespace ElasticSearch.Core.Services;

public class ProductSearchService
{
    private readonly ElasticClient _elasticClient;

    public ProductSearchService(ElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task<List<Product>> SearchProductsAsync(string query)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .MultiMatch(m => m
                    .Query(query)
                    .Fields(f => f
                        .Field(p => p.Name)
                        .Field(p => p.Description)
                        .Field(p => p.Category)
                    )
                )
            )
        );

        if (!response.IsValid || response.Documents == null)
        {
            return new List<Product>();
        }

        return response.Documents.ToList();
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        var response = await _elasticClient.GetAsync<Product>(id, g => g.Index("products"));
        return response.Found ? response.Source : null;
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Term(t => t.Field(f => f.Category).Value(category))
            )
        );

        if (!response.IsValid || response.Documents == null)
        {
            return new List<Product>();
        }

        return response.Documents.ToList();
    }

    public async Task<List<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Range(r => r
                    .Field(f => f.Price)
                    .GreaterThanOrEquals((double)minPrice)
                    .LessThanOrEquals((double)maxPrice)
                )
            )
        );

        if (!response.IsValid || response.Documents == null)
        {
            return new List<Product>();
        }

        return response.Documents.ToList();
    }

    public async Task<Dictionary<string, long>> GetProductCountByCategoryAsync()
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Size(0)
            .Aggregations(a => a
                .Terms("categories", t => t
                    .Field(f => f.Category)
                    .Size(50)
                )
            )
        );

        if (!response.IsValid || response.Aggregations == null)
        {
            return new Dictionary<string, long>();
        }

        var termsAggregation = response.Aggregations.Terms("categories");
        if (termsAggregation?.Buckets == null)
        {
            return new Dictionary<string, long>();
        }

        return termsAggregation.Buckets.ToDictionary(b => b.Key, b => b.DocCount ?? 0);
    }
}

