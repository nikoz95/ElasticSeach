using Nest;
using ElasticSearch.Core.Models;

namespace ElasticSearch.Core.Services;

public class ProductSearchService(ElasticClient elasticClient)
{
    public async Task<List<Product>> SearchProductsAsync(string query)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .MultiMatch(m => m // match on multiple fields
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
            return [];
        }

        return response.Documents.ToList();
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        var response = await elasticClient.GetAsync<Product>(id, g => g.Index("products"));
        return response.Found ? response.Source : null;
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Query(q => q
                .Term(t => t.Field(f => f.Category.Suffix("keyword")).Value(category.ToLower()))
            )
        );

        if (!response.IsValid || response.Documents == null)
        {
            return [];
        }

        return response.Documents.ToList();
    }

    public async Task<List<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
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
            return [];
        }

        return response.Documents.ToList();
    }

    public async Task<Dictionary<string, long>> GetProductCountByCategoryAsync()
    {
        var response = await elasticClient.SearchAsync<Product>(s => s
            .Index("products")
            .Size(0)
            .Aggregations(a => a
                .Terms("categories", t => t
                    .Field(f => f.Category.Suffix("keyword"))
                    .Size(50)
                )
            )
        );

        if (!response.IsValid || response.Aggregations == null)
        {
            return new Dictionary<string, long>();
        }

        var termsAggregation = response.Aggregations.Terms("categories");
        return termsAggregation?.Buckets == null ? new Dictionary<string, long>() : termsAggregation.Buckets.ToDictionary(b => b.Key, b => b.DocCount ?? 0);
    }
}

