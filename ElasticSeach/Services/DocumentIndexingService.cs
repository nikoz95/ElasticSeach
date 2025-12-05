using Nest;
using ElasticSeach.Models;

namespace ElasticSeach.Services;

/// <summary>
/// 4. Indexing Documents
/// Demonstrates different ways to index documents
/// </summary>
public class DocumentIndexingService
{
    private readonly ElasticClient _client;

    public DocumentIndexingService(ElasticClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Index single document
    /// </summary>
    public async Task IndexSingleDocumentAsync()
    {
        Console.WriteLine("\n=== 4. Indexing Documents ===");
        Console.WriteLine("4.1. Index Single Document");

        var product = new Product
        {
            Id = "1",
            Name = "Laptop Dell XPS 15",
            Description = "High-performance laptop with Intel Core i7 processor and 16GB RAM",
            Price = 1499.99m,
            Stock = 25,
            Category = "Electronics",
            Tags = new List<string> { "laptop", "computer", "dell", "premium" },
            CreatedDate = DateTime.Now,
            IsActive = true,
            Specifications = new ProductSpecs
            {
                Brand = "Dell",
                Model = "XPS 15",
                Features = new Dictionary<string, string>
                {
                    { "Processor", "Intel Core i7" },
                    { "RAM", "16GB" },
                    { "Storage", "512GB SSD" }
                }
            }
        };

        var indexResponse = await _client.IndexAsync(product, i => i
            .Index("products")
            .Id(product.Id)
            .Refresh(Elasticsearch.Net.Refresh.WaitFor)
        );

        if (indexResponse.IsValid)
        {
            Console.WriteLine($"✓ Document indexed with ID: {indexResponse.Id}");
            Console.WriteLine($"  Index: {indexResponse.Index}");
            Console.WriteLine($"  Version: {indexResponse.Version}");
        }
        else
        {
            Console.WriteLine($"✗ Error: {indexResponse.DebugInformation}");
        }
    }

    /// <summary>
    /// Bulk indexing - efficient way to index multiple documents
    /// </summary>
    public async Task BulkIndexDocumentsAsync()
    {
        Console.WriteLine("\n4.2. Bulk Indexing (multiple documents)");

        var products = new List<Product>
        {
            new Product
            {
                Id = "2",
                Name = "iPhone 15 Pro",
                Description = "Latest Apple smartphone with A17 Pro chip and advanced camera system",
                Price = 999.99m,
                Stock = 50,
                Category = "Electronics",
                Tags = new List<string> { "smartphone", "apple", "iphone", "mobile" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Apple",
                    Model = "iPhone 15 Pro",
                    Features = new Dictionary<string, string>
                    {
                        { "Chip", "A17 Pro" },
                        { "Storage", "256GB" },
                        { "Display", "6.1 inch" }
                    }
                }
            },
            new Product
            {
                Id = "3",
                Name = "Samsung Galaxy S24",
                Description = "Powerful Android phone with AI features and excellent display",
                Price = 899.99m,
                Stock = 40,
                Category = "Electronics",
                Tags = new List<string> { "smartphone", "samsung", "android", "galaxy" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Samsung",
                    Model = "Galaxy S24",
                    Features = new Dictionary<string, string>
                    {
                        { "Processor", "Snapdragon 8 Gen 3" },
                        { "RAM", "12GB" },
                        { "Display", "6.2 inch AMOLED" }
                    }
                }
            },
            new Product
            {
                Id = "4",
                Name = "Sony WH-1000XM5",
                Description = "Premium noise-cancelling wireless headphones",
                Price = 399.99m,
                Stock = 30,
                Category = "Audio",
                Tags = new List<string> { "headphones", "sony", "wireless", "noise-cancelling" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Sony",
                    Model = "WH-1000XM5",
                    Features = new Dictionary<string, string>
                    {
                        { "Type", "Over-ear" },
                        { "Battery", "30 hours" },
                        { "Connectivity", "Bluetooth 5.2" }
                    }
                }
            },
            new Product
            {
                Id = "5",
                Name = "iPad Pro 12.9",
                Description = "Professional tablet with M2 chip and Liquid Retina display",
                Price = 1099.99m,
                Stock = 20,
                Category = "Electronics",
                Tags = new List<string> { "tablet", "apple", "ipad", "professional" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Apple",
                    Model = "iPad Pro",
                    Features = new Dictionary<string, string>
                    {
                        { "Chip", "M2" },
                        { "Storage", "256GB" },
                        { "Display", "12.9 inch Liquid Retina" }
                    }
                }
            },
            new Product
            {
                Id = "6",
                Name = "MacBook Air M3",
                Description = "Ultra-thin laptop with powerful M3 chip and all-day battery life",
                Price = 1299.99m,
                Stock = 15,
                Category = "Electronics",
                Tags = new List<string> { "laptop", "apple", "macbook", "lightweight" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Apple",
                    Model = "MacBook Air",
                    Features = new Dictionary<string, string>
                    {
                        { "Chip", "M3" },
                        { "RAM", "16GB" },
                        { "Storage", "512GB SSD" }
                    }
                }
            },
            new Product
            {
                Id = "7",
                Name = "Logitech MX Master 3S",
                Description = "Ergonomic wireless mouse for productivity",
                Price = 99.99m,
                Stock = 45,
                Category = "Accessories",
                Tags = new List<string> { "mouse", "logitech", "wireless", "ergonomic" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Logitech",
                    Model = "MX Master 3S",
                    Features = new Dictionary<string, string>
                    {
                        { "Type", "Wireless" },
                        { "DPI", "8000" },
                        { "Battery", "70 days" }
                    }
                }
            },
            new Product
            {
                Id = "8",
                Name = "Bose QuietComfort Earbuds II",
                Description = "True wireless earbuds with world-class noise cancellation",
                Price = 279.99m,
                Stock = 35,
                Category = "Audio",
                Tags = new List<string> { "earbuds", "bose", "wireless", "noise-cancelling" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Bose",
                    Model = "QuietComfort Earbuds II",
                    Features = new Dictionary<string, string>
                    {
                        { "Type", "In-ear" },
                        { "Battery", "6 hours" },
                        { "Connectivity", "Bluetooth 5.3" }
                    }
                }
            },
            new Product
            {
                Id = "9",
                Name = "USB-C Cable 3-Pack",
                Description = "Fast charging USB-C cables, 6ft length",
                Price = 19.99m,
                Stock = 100,
                Category = "Accessories",
                Tags = new List<string> { "cable", "usb-c", "charging", "fast-charge" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Generic",
                    Model = "USB-C 3.0",
                    Features = new Dictionary<string, string>
                    {
                        { "Length", "6 feet" },
                        { "Speed", "USB 3.0" },
                        { "Power", "100W" }
                    }
                }
            },
            new Product
            {
                Id = "10",
                Name = "Samsung 27\" 4K Monitor",
                Description = "Ultra HD monitor with HDR support for professionals",
                Price = 449.99m,
                Stock = 12,
                Category = "Electronics",
                Tags = new List<string> { "monitor", "samsung", "4k", "display" },
                CreatedDate = DateTime.Now,
                IsActive = true,
                Specifications = new ProductSpecs
                {
                    Brand = "Samsung",
                    Model = "U28E590D",
                    Features = new Dictionary<string, string>
                    {
                        { "Size", "27 inch" },
                        { "Resolution", "3840x2160" },
                        { "Panel", "IPS" }
                    }
                }
            }
        };

        var bulkResponse = await _client.BulkAsync(b => b
            .Index("products")
            .IndexMany(products, (descriptor, product) => descriptor
                .Id(product.Id)
            )
            .Refresh(Elasticsearch.Net.Refresh.WaitFor)
        );

        // Check if there are actual errors (not just IsValid flag)
        // Status 201 = created, 200 = updated - both are success
        var actualErrors = bulkResponse.ItemsWithErrors?.Where(i => i.Error != null).ToList();
        var hasRealErrors = actualErrors != null && actualErrors.Any();

        if (!hasRealErrors && bulkResponse.Items != null && bulkResponse.Items.Any())
        {
            // Success! Count how many were created vs updated
            var created = bulkResponse.Items.Count(i => i.Status == 201);
            var updated = bulkResponse.Items.Count(i => i.Status == 200);
            
            Console.WriteLine($"✓ Bulk indexed {products.Count} documents");
            Console.WriteLine($"  Created: {created}, Updated: {updated}");
            Console.WriteLine($"  Took: {bulkResponse.Took}ms");
        }
        else if (hasRealErrors)
        {
            Console.WriteLine($"✗ Bulk indexing had errors!");
            Console.WriteLine($"  Successful: {bulkResponse.Items.Count(i => i.IsValid)}");
            Console.WriteLine($"  Failed: {actualErrors.Count}");
            
            Console.WriteLine("\nErrors:");
            foreach (var item in actualErrors)
            {
                var errorMsg = item.Error != null 
                    ? $"{item.Error.Type}: {item.Error.Reason}" 
                    : "Unknown error";
                Console.WriteLine($"  - Document {item.Id}: {errorMsg}");
            }
        }
        else
        {
            // Fallback if response is completely invalid
            Console.WriteLine($"✗ Bulk request failed!");
            Console.WriteLine($"  Error: {bulkResponse.ServerError?.Error?.Reason ?? "Unknown error"}");
        }
    }

    /// <summary>
    /// Update document
    /// </summary>
    public async Task UpdateDocumentAsync()
    {
        Console.WriteLine("\n4.3. Update Document");

        var updateResponse = await _client.UpdateAsync<Product>("1", u => u
            .Index("products")
            .Doc(new Product { Price = 1399.99m, Stock = 20 })
            .Refresh(Elasticsearch.Net.Refresh.WaitFor)
        );

        if (updateResponse.IsValid)
        {
            Console.WriteLine($"✓ Document updated");
            Console.WriteLine($"  New version: {updateResponse.Version}");
        }
    }

    /// <summary>
    /// Delete document
    /// </summary>
    public async Task DeleteDocumentAsync(string id)
    {
        Console.WriteLine($"\n4.4. Delete Document (ID: {id})");

        var deleteResponse = await _client.DeleteAsync<Product>(id, d => d
            .Index("products")
            .Refresh(Elasticsearch.Net.Refresh.WaitFor)
        );

        if (deleteResponse.IsValid)
        {
            Console.WriteLine($"✓ Document deleted: {deleteResponse.Result}");
        }
    }

    /// <summary>
    /// Index articles for text analysis demonstration
    /// </summary>
    public async Task IndexArticlesAsync()
    {
        Console.WriteLine("\n4.5. Indexing Articles for Text Analysis");

        var articles = new List<Article>
        {
            new Article
            {
                Id = "1",
                Title = "Introduction to Elasticsearch",
                Content = "Elasticsearch is a distributed, RESTful search and analytics engine. It provides real-time search and analytics for all types of data.",
                Author = "John Doe",
                PublishDate = DateTime.Now.AddDays(-10),
                Keywords = new List<string> { "elasticsearch", "search", "analytics" },
                ViewCount = 150
            },
            new Article
            {
                Id = "2",
                Title = "Understanding Elasticsearch Mappings",
                Content = "Mappings define how documents and their fields are stored and indexed. Each field has a data type that determines how it can be searched.",
                Author = "Jane Smith",
                PublishDate = DateTime.Now.AddDays(-5),
                Keywords = new List<string> { "elasticsearch", "mappings", "indexing" },
                ViewCount = 200
            },
            new Article
            {
                Id = "3",
                Title = "Query DSL in Elasticsearch",
                Content = "The Query DSL is a flexible, expressive search language that Elasticsearch uses to expose most of the power of Lucene through a simple JSON interface.",
                Author = "Bob Johnson",
                PublishDate = DateTime.Now.AddDays(-2),
                Keywords = new List<string> { "elasticsearch", "query", "dsl", "search" },
                ViewCount = 180
            }
        };

        var bulkResponse = await _client.BulkAsync(b => b
            .Index("articles")
            .IndexMany(articles, (descriptor, article) => descriptor.Id(article.Id))
            .Refresh(Elasticsearch.Net.Refresh.WaitFor)
        );

        if (bulkResponse.IsValid)
        {
            Console.WriteLine($"✓ Indexed {articles.Count} articles");
        }
    }
}

