using Nest;
using Microsoft.Data.SqlClient;
using Dapper;
using ElasticSearch.Core.Models;

namespace ElasticSearch.Core.Services;

public class SqlToElasticsearchSyncService
{
    private readonly ElasticClient _elasticClient;
    private readonly string _sqlConnectionString;

    public SqlToElasticsearchSyncService(ElasticClient elasticClient, string sqlConnectionString)
    {
        _elasticClient = elasticClient;
        _sqlConnectionString = sqlConnectionString;
    }

    public async Task FullSyncAsync()
    {
        var startTime = DateTime.Now;
        Console.WriteLine($"🔄 [FULL SYNC] Starting at {startTime:HH:mm:ss}...");
        
        try
        {
            await EnsureIndexExistsAsync();
            Console.WriteLine("  ✓ Index ensured");
            
            var products = await GetProductsFromSqlAsync();
            Console.WriteLine($"  📊 Found {products.Count} products in SQL Server");
            
            if (products.Count > 0)
            {
                await BulkIndexToElasticsearchAsync(products);
                Console.WriteLine($"  ✅ Indexed {products.Count} products to Elasticsearch");
            }
            else
            {
                Console.WriteLine("  ⚠️  No products found to sync");
            }
            
            var duration = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"✅ [FULL SYNC] Completed in {duration:F2}s");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [FULL SYNC] Failed: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
            throw;
        }
    }

    public async Task IncrementalSyncAsync()
    {
        Console.WriteLine("🔄 [INCREMENTAL SYNC] Starting...");
        
        var lastSync = await GetLastSyncTimestampAsync();
        Console.WriteLine($"📅 Last sync: {lastSync:yyyy-MM-dd HH:mm:ss}");
        
        var changes = await GetChangedProductsFromSqlAsync(lastSync);
        Console.WriteLine($"📊 Found {changes.Count} changes");
        
        if (changes.Count == 0)
        {
            await UpdateLastSyncTimestampAsync(DateTime.UtcNow);
            Console.WriteLine("✅ No changes");
            return;
        }

        foreach (var change in changes)
        {
            if (change.IsDeleted)
            {
                await _elasticClient.DeleteAsync<Product>(change.Id.ToString(), d => d.Index("products"));
                Console.WriteLine($"  ✗ Deleted product {change.Id}");
            }
            else
            {
                var product = await GetProductDetailFromSqlAsync(change.Id);
                if (product != null)
                {
                    await _elasticClient.IndexAsync(product, i => i.Index("products").Id(product.Id));
                    Console.WriteLine($"  ✓ Synced product {change.Id}");
                }
            }
        }

        await UpdateLastSyncTimestampAsync(DateTime.UtcNow);
        Console.WriteLine("✅ [INCREMENTAL SYNC] Completed");
    }

    private async Task EnsureIndexExistsAsync()
    {
        var exists = await _elasticClient.Indices.ExistsAsync("products");
        if (!exists.Exists)
        {
            await _elasticClient.Indices.CreateAsync("products", c => c
                .Map<Product>(m => m.AutoMap())
            );
        }
    }

    private async Task<List<Product>> GetProductsFromSqlAsync()
    {
        await using var connection = new SqlConnection(_sqlConnectionString);
        
        var sql = "SELECT * FROM Products WHERE IsActive = 1";
        var dtos = (await connection.QueryAsync<ProductDto>(sql)).ToList();
        
        return dtos.Select(dto => new Product
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            Tags = string.IsNullOrEmpty(dto.Tags) ? new List<string>() : dto.Tags.Split(',').ToList(),
            CreatedDate = dto.CreatedDate,
            IsActive = dto.IsActive,
            Specifications = new ProductSpecs { Brand = dto.Brand, Model = dto.Model }
        }).ToList();
    }

    private async Task<List<ProductChange>> GetChangedProductsFromSqlAsync(DateTime lastSync)
    {
        await using var connection = new SqlConnection(_sqlConnectionString);
        var sql = "SELECT Id, UpdatedAt, CreatedDate, IsDeleted FROM Products WHERE UpdatedAt > @LastSync";
        return (await connection.QueryAsync<ProductChange>(sql, new { LastSync = lastSync })).ToList();
    }

    private async Task<Product?> GetProductDetailFromSqlAsync(int id)
    {
        await using var connection = new SqlConnection(_sqlConnectionString);
        var dto = await connection.QueryFirstOrDefaultAsync<ProductDto>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        
        if (dto == null) return null;
        
        return new Product
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            Tags = string.IsNullOrEmpty(dto.Tags) ? new List<string>() : dto.Tags.Split(',').ToList(),
            CreatedDate = dto.CreatedDate,
            IsActive = dto.IsActive,
            Specifications = new ProductSpecs { Brand = dto.Brand, Model = dto.Model }
        };
    }

    private async Task BulkIndexToElasticsearchAsync(List<Product> products)
    {
        const int batchSize = 1000;
        var batches = products.Chunk(batchSize).ToList();
        
        Console.WriteLine($"  📦 Indexing {products.Count} products in {batches.Count} batch(es)...");
        
        int processed = 0;
        int successCount = 0;
        int errorCount = 0;
        
        foreach (var batch in batches)
        {
            var response = await _elasticClient.BulkAsync(b => b.Index("products").IndexMany(batch));
            
            if (!response.IsValid)
            {
                errorCount += batch.Length;
                Console.WriteLine($"  ❌ Bulk index failed: {response.OriginalException?.Message}");
                Console.WriteLine($"     Server error: {response.ServerError?.Error?.Reason}");
                Console.WriteLine($"     Debug info: {response.DebugInformation}");
            }
            else if (response.Errors)
            {
                // Some items failed
                var itemsWithErrors = response.ItemsWithErrors.ToList();
                errorCount += itemsWithErrors.Count;
                successCount += batch.Length - itemsWithErrors.Count;
                
                foreach (var item in itemsWithErrors.Take(3)) // Show first 3 errors
                {
                    Console.WriteLine($"  ⚠️  Item error: {item.Error?.Reason ?? "Unknown"}");
                }
                
                if (itemsWithErrors.Count > 3)
                {
                    Console.WriteLine($"  ⚠️  ... and {itemsWithErrors.Count - 3} more errors");
                }
            }
            else
            {
                successCount += batch.Length;
            }
            
            processed += batch.Length;
            Console.WriteLine($"    → Processed {processed}/{products.Count} products (✓ {successCount} success, ✗ {errorCount} errors)");
            
            await Task.Delay(100);
        }
    }

    private async Task<DateTime> GetLastSyncTimestampAsync()
    {
        try
        {
            var response = await _elasticClient.GetAsync<SyncMetadata>("last_sync", g => g.Index("sync_metadata"));
            return response.Found && response.Source != null ? response.Source.LastSync : DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private async Task UpdateLastSyncTimestampAsync(DateTime timestamp)
    {
        var exists = await _elasticClient.Indices.ExistsAsync("sync_metadata");
        if (!exists.Exists)
        {
            await _elasticClient.Indices.CreateAsync("sync_metadata");
        }

        await _elasticClient.IndexAsync(new SyncMetadata
        {
            Id = "last_sync",
            LastSync = timestamp,
            SyncType = "incremental"
        }, i => i.Index("sync_metadata").Id("last_sync"));
    }
}

