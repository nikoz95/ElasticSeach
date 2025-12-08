using Microsoft.Data.SqlClient;
using Dapper;

namespace ElasticSearch.Core.Services;

/// <summary>
/// Database seeder service - initializes database and seeds test data
/// </summary>
public class DatabaseSeederService(string connectionString)
{
    /// <summary>
    /// Initialize database: create tables if not exist and seed test data
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        Console.WriteLine("🔧 Initializing database...");
        
        try
        {
            await CreateDatabaseIfNotExistsAsync();
            await CreateTablesAsync();
            await SeedTestDataAsync();
            Console.WriteLine("✅ Database initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Create database if it doesn't exist
    /// </summary>
    private async Task CreateDatabaseIfNotExistsAsync()
    {
        // Parse connection string to get database name and create master connection
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        
        // Connect to master database
        builder.InitialCatalog = "master";
        var masterConnectionString = builder.ConnectionString;
        
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        
        // Check if database exists
        var checkDbSql = $"SELECT database_id FROM sys.databases WHERE name = '{databaseName}'";
        var dbId = await connection.ExecuteScalarAsync<int?>(checkDbSql);
        
        if (dbId == null)
        {
            // Create database
            var createDbSql = $"CREATE DATABASE [{databaseName}]";
            await connection.ExecuteAsync(createDbSql);
            Console.WriteLine($"  ✅ Database '{databaseName}' created");
        }
        else
        {
            Console.WriteLine($"  ✓ Database '{databaseName}' already exists");
        }
    }

    /// <summary>
    /// Create Products table if not exists
    /// </summary>
    private async Task CreateTablesAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
            BEGIN
                CREATE TABLE Products (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(255) NOT NULL,
                    Description NVARCHAR(MAX) NULL,
                    Price DECIMAL(18,2) NOT NULL,
                    Stock INT NOT NULL DEFAULT 0,
                    Category NVARCHAR(100) NOT NULL,
                    Tags NVARCHAR(500) NULL,
                    Brand NVARCHAR(100) NULL,
                    Model NVARCHAR(100) NULL,
                    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                    IsActive BIT NOT NULL DEFAULT 1,
                    IsDeleted BIT NOT NULL DEFAULT 0
                );
                
                CREATE INDEX IX_Products_Category ON Products(Category);
                CREATE INDEX IX_Products_UpdatedAt ON Products(UpdatedAt);
                CREATE INDEX IX_Products_IsActive ON Products(IsActive);
                
                PRINT '✅ Products table created';
            END
            ELSE
            BEGIN
                PRINT '✓ Products table already exists';
            END";

        await connection.ExecuteAsync(createTableSql);
        Console.WriteLine("  ✓ Products table ready");
    }

    /// <summary>
    /// Seed test data if table is empty
    /// </summary>
    private async Task SeedTestDataAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        
        // Check if data already exists
        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Products");
        
        if (count > 0)
        {
            Console.WriteLine($"  ✓ Database already has {count} products - skipping seed");
            return;
        }

        Console.WriteLine("  📦 Seeding test data...");

        var testProducts = GetTestProducts();
        
        const string insertSql = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, Tags, Brand, Model, CreatedDate, UpdatedAt, IsActive, IsDeleted)
            VALUES (@Name, @Description, @Price, @Stock, @Category, @Tags, @Brand, @Model, @CreatedDate, @UpdatedAt, @IsActive, @IsDeleted)";

        await connection.ExecuteAsync(insertSql, testProducts);
        
        Console.WriteLine($"  ✅ Seeded {testProducts.Count} test products");
    }

    /// <summary>
    /// Generate test product data
    /// </summary>
    private static List<object> GetTestProducts()
    {
        var now = DateTime.Now;
        
        return
        [
            new
            {
                Name = "MacBook Pro 16\" M3",
                Description = "ძლიერი ლეპტოპი პროფესიონალებისთვის Apple M3 ჩიპით, 16GB RAM და 512GB SSD",
                Price = 4999.99m,
                Stock = 15,
                Category = "Laptops",
                Tags = "apple,laptop,macbook,premium,m3",
                Brand = "Apple",
                Model = "MacBook Pro 16\" 2024",
                CreatedDate = now.AddDays(-30),
                UpdatedAt = now.AddDays(-30),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Dell XPS 15",
                Description = "მაღალი ხარისხის ლეპტოპი Intel Core i7, 16GB RAM, 1TB SSD",
                Price = 2499.99m,
                Stock = 25,
                Category = "Laptops",
                Tags = "dell,laptop,xps,business,premium",
                Brand = "Dell",
                Model = "XPS 15 9530",
                CreatedDate = now.AddDays(-25),
                UpdatedAt = now.AddDays(-25),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "iPhone 15 Pro Max",
                Description = "უახლესი iPhone A17 Pro ჩიპით, 256GB, Titanium design",
                Price = 2799.99m,
                Stock = 50,
                Category = "Smartphones",
                Tags = "apple,iphone,smartphone,5g,premium",
                Brand = "Apple",
                Model = "iPhone 15 Pro Max",
                CreatedDate = now.AddDays(-20),
                UpdatedAt = now.AddDays(-20),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Samsung Galaxy S24 Ultra",
                Description = "ფლაგმანი სმარტფონი Samsung-დან, Snapdragon 8 Gen 3, 512GB",
                Price = 2399.99m,
                Stock = 35,
                Category = "Smartphones",
                Tags = "samsung,galaxy,smartphone,android,premium",
                Brand = "Samsung",
                Model = "Galaxy S24 Ultra",
                CreatedDate = now.AddDays(-18),
                UpdatedAt = now.AddDays(-18),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Sony WH-1000XM5",
                Description = "პრემიუმ უკაბელო ყურსასმენი შესანიშნავი ხმის დაბლოკვით",
                Price = 399.99m,
                Stock = 100,
                Category = "Audio",
                Tags = "sony,headphones,wireless,noise-cancelling,premium",
                Brand = "Sony",
                Model = "WH-1000XM5",
                CreatedDate = now.AddDays(-15),
                UpdatedAt = now.AddDays(-15),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "iPad Pro 12.9\" M2",
                Description = "მძლავრი ტაბლეტი M2 ჩიპით, 256GB, Liquid Retina XDR დისპლეი",
                Price = 1899.99m,
                Stock = 20,
                Category = "Tablets",
                Tags = "apple,ipad,tablet,m2,premium",
                Brand = "Apple",
                Model = "iPad Pro 12.9\" 2023",
                CreatedDate = now.AddDays(-12),
                UpdatedAt = now.AddDays(-12),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "LG 55\" OLED C3",
                Description = "პრემიუმ OLED ტელევიზორი 4K, 120Hz, WebOS Smart TV",
                Price = 1799.99m,
                Stock = 12,
                Category = "TVs",
                Tags = "lg,tv,oled,4k,smart-tv",
                Brand = "LG",
                Model = "OLED55C3",
                CreatedDate = now.AddDays(-10),
                UpdatedAt = now.AddDays(-10),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Logitech MX Master 3S",
                Description = "ერგონომიული უკაბელო მაუსი პროფესიონალებისთვის",
                Price = 99.99m,
                Stock = 150,
                Category = "Accessories",
                Tags = "logitech,mouse,wireless,ergonomic,productivity",
                Brand = "Logitech",
                Model = "MX Master 3S",
                CreatedDate = now.AddDays(-8),
                UpdatedAt = now.AddDays(-8),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "HP LaserJet Pro M404dn",
                Description = "სამუშაო პრინტერი ოფისისთვის, ბეჭდვის სიჩქარე 40 გვერდი/წუთში",
                Price = 299.99m,
                Stock = 30,
                Category = "Printers",
                Tags = "hp,printer,laser,office,business",
                Brand = "HP",
                Model = "LaserJet Pro M404dn",
                CreatedDate = now.AddDays(-7),
                UpdatedAt = now.AddDays(-7),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Kingston Fury 32GB DDR5",
                Description = "მაღალსიჩქარიანი RAM 5600MHz, 2x16GB Kit",
                Price = 149.99m,
                Stock = 80,
                Category = "Components",
                Tags = "kingston,ram,memory,ddr5,gaming",
                Brand = "Kingston",
                Model = "Fury Beast DDR5",
                CreatedDate = now.AddDays(-6),
                UpdatedAt = now.AddDays(-6),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Samsung 990 PRO 2TB",
                Description = "სწრაფი NVMe SSD, PCIe 4.0, 7450 MB/s read speed",
                Price = 249.99m,
                Stock = 60,
                Category = "Storage",
                Tags = "samsung,ssd,nvme,storage,fast",
                Brand = "Samsung",
                Model = "990 PRO",
                CreatedDate = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "ASUS ROG Strix RTX 4080",
                Description = "გეიმერული ვიდეოკარტა NVIDIA RTX 4080, 16GB GDDR6X",
                Price = 1599.99m,
                Stock = 8,
                Category = "Components",
                Tags = "asus,gpu,graphics-card,nvidia,gaming",
                Brand = "ASUS",
                Model = "ROG Strix RTX 4080",
                CreatedDate = now.AddDays(-4),
                UpdatedAt = now.AddDays(-4),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Razer BlackWidow V4 Pro",
                Description = "მექანიკური გეიმერული კლავიატურა RGB განათებით",
                Price = 229.99m,
                Stock = 45,
                Category = "Gaming",
                Tags = "razer,keyboard,mechanical,gaming,rgb",
                Brand = "Razer",
                Model = "BlackWidow V4 Pro",
                CreatedDate = now.AddDays(-3),
                UpdatedAt = now.AddDays(-3),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Lenovo ThinkPad X1 Carbon",
                Description = "ბიზნეს ლეპტოპი Intel Core i7, 16GB RAM, მსუბუქი და გამძლე",
                Price = 1899.99m,
                Stock = 18,
                Category = "Laptops",
                Tags = "lenovo,laptop,thinkpad,business,portable",
                Brand = "Lenovo",
                Model = "ThinkPad X1 Carbon Gen 11",
                CreatedDate = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2),
                IsActive = true,
                IsDeleted = false
            },

            new
            {
                Name = "Google Pixel 8 Pro",
                Description = "Android smartphone Google-ისგან, Tensor G3, 256GB, შესანიშნავი კამერა",
                Price = 999.99m,
                Stock = 40,
                Category = "Smartphones",
                Tags = "google,pixel,smartphone,android,camera",
                Brand = "Google",
                Model = "Pixel 8 Pro",
                CreatedDate = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1),
                IsActive = true,
                IsDeleted = false
            }
        ];
    }
}

