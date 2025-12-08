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
            
            // Update last sync timestamp
            await UpdateLastSyncTimestampAsync(DateTime.UtcNow);
            Console.WriteLine("  ✓ Updated last sync timestamp");
            
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
        var startTime = DateTime.Now;
        Console.WriteLine($"🔄 [INCREMENTAL SYNC] Starting at {startTime:HH:mm:ss}...");
        
        try
        {
            var lastSync = await GetLastSyncTimestampAsync();
            var timeSinceLastSync = DateTime.UtcNow - lastSync;
            
            Console.WriteLine($"  📅 Last sync: {lastSync:yyyy-MM-dd HH:mm:ss} UTC ({timeSinceLastSync.TotalMinutes:F1} minutes ago)");
            
            var changes = await GetChangedProductsFromSqlAsync(lastSync);
            Console.WriteLine($"  📊 Found {changes.Count} changes since last sync");
            
            if (changes.Count == 0)
            {
                await UpdateLastSyncTimestampAsync(DateTime.UtcNow);
                Console.WriteLine("  ✅ No changes to sync");
                var duration = (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine($"✅ [INCREMENTAL SYNC] Completed in {duration:F2}s");
                return;
            }

            int syncedCount = 0;
            int deletedCount = 0;
            
            foreach (var change in changes)
            {
                if (change.IsDeleted)
                {
                    await _elasticClient.DeleteAsync<Product>(change.Id.ToString(), d => d.Index("products"));
                    deletedCount++;
                    Console.WriteLine($"  ✗ Deleted product {change.Id}");
                }
                else
                {
                    var product = await GetProductDetailFromSqlAsync(change.Id);
                    if (product != null)
                    {
                        await _elasticClient.IndexAsync(product, i => i.Index("products").Id(product.Id));
                        syncedCount++;
                        Console.WriteLine($"  ✓ Synced product {change.Id}: {product.Name}");
                    }
                }
            }

            await UpdateLastSyncTimestampAsync(DateTime.UtcNow);
            
            var totalDuration = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"  📈 Summary: {syncedCount} updated, {deletedCount} deleted");
            Console.WriteLine($"✅ [INCREMENTAL SYNC] Completed in {totalDuration:F2}s");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [INCREMENTAL SYNC] Failed: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
            throw;
        }
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
            
            // Don't trust response.IsValid or response.Errors flags
            // They are unreliable with NEST library
            // Instead, check actual HTTP status codes
            
            if (response.Items != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                {
                    // Status 200 (OK) or 201 (Created) = SUCCESS
                    // Anything else = ERROR
                    if (item.Status >= 200 && item.Status < 300)
                    {
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                        if (errorCount <= 3)
                        {
                            var errorMsg = item.Error?.Reason ?? "Unknown error";
                            Console.WriteLine($"  ⚠️  Item {item.Id} failed (HTTP {item.Status}): {errorMsg}");
                        }
                    }
                }
                
                if (errorCount > 3)
                {
                    Console.WriteLine($"  ⚠️  ... and {errorCount - 3} more errors");
                }
            }
            else if (response.OriginalException != null)
            {
                // Complete failure - network/connection error
                errorCount += batch.Length;
                Console.WriteLine($"  ❌ Bulk request exception: {response.OriginalException.Message}");
            }
            else
            {
                // No items in response but no exception - assume success
                successCount += batch.Length;
            }
            
            processed += batch.Length;
            
            if (errorCount > 0)
            {
                Console.WriteLine($"    → Processed {processed}/{products.Count} (✓ {successCount} success, ✗ {errorCount} failed)");
            }
            else
            {
                Console.WriteLine($"    → Processed {processed}/{products.Count} (✓ {successCount} success)");
            }
            
            await Task.Delay(100);
        }
    }

    private async Task<DateTime> GetLastSyncTimestampAsync()
    {
        try
        {
            var exists = await _elasticClient.Indices.ExistsAsync("sync_metadata");
            if (!exists.Exists)
            {
                // First run - sync everything from last 24 hours
                return DateTime.UtcNow.AddDays(-1);
            }
            
            var response = await _elasticClient.GetAsync<SyncMetadata>("last_sync", g => g.Index("sync_metadata"));
            
            if (response.Found && response.Source != null)
            {
                return response.Source.LastSync;
            }
            
            // Index exists but no last_sync document - sync from last 24 hours
            return DateTime.UtcNow.AddDays(-1);
        }
        catch
        {
            // On error, sync from last 24 hours to be safe
            return DateTime.UtcNow.AddDays(-1);
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

