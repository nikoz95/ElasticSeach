namespace ElasticSeach.Models;

/// <summary>
/// Product document model - demonstrates different data types and mappings
/// </summary>
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
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Dictionary<string, string> Features { get; set; } = new();
}

