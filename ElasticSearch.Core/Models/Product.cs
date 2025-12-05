namespace ElasticSearch.Core.Models;

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public ProductSpecs? Specifications { get; set; }
}

public class ProductSpecs
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
}

public class ProductChange
{
    public int Id { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class SyncMetadata
{
    public string Id { get; set; } = string.Empty;
    public DateTime LastSync { get; set; }
    public string SyncType { get; set; } = string.Empty;
}

