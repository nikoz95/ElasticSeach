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

        var testProducts = TestProductsProvider.GetTestProducts();
        
        const string insertSql = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, Tags, Brand, Model, CreatedDate, UpdatedAt, IsActive, IsDeleted)
            VALUES (@Name, @Description, @Price, @Stock, @Category, @Tags, @Brand, @Model, @CreatedDate, @UpdatedAt, @IsActive, @IsDeleted)";

        await connection.ExecuteAsync(insertSql, testProducts);
        
        Console.WriteLine($"  ✅ Seeded {testProducts.Count} test products");
    }
}

