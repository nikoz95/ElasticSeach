using Microsoft.AspNetCore.Mvc;
using ElasticSearch.Core.Models;
using ElasticSearch.Core.Services;

namespace ElasticSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductSearchService _searchService;

    public ProductsController(ProductSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Search products - მარტივი ძებნა სახელის და აღწერის მიხედვით
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     GET /api/products/search?query=macbook
    ///     
    /// Response: MacBook Pro პროდუქტების სია
    /// </remarks>
    [HttpGet("search")]
    public async Task<ActionResult<List<Product>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query))
            return BadRequest("Query is required");

        var products = await _searchService.SearchProductsAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID - კონკრეტული პროდუქტის მოძებნა ID-ის მიხედვით
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     GET /api/products/1
    ///     
    /// Response: პროდუქტი ID=1 (MacBook Pro 16" M3)
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id)
    {
        var product = await _searchService.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Get products by category - კატეგორიის მიხედვით ფილტრაცია
    /// </summary>
    /// <remarks>
    /// Example requests:
    /// 
    ///     GET /api/products/category/laptops
    ///     GET /api/products/category/smartphones
    ///     GET /api/products/category/audio
    ///     
    /// Response: კატეგორიის ყველა პროდუქტი
    /// </remarks>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<Product>>> GetByCategory(string category)
    {
        var products = await _searchService.GetProductsByCategoryAsync(category);
        return Ok(products);
    }

    /// <summary>
    /// Get products by price range - ფასის დიაპაზონით ფილტრაცია
    /// </summary>
    /// <remarks>
    /// Example requests:
    /// 
    ///     GET /api/products/price-range?min=1000&amp;max=3000
    ///     GET /api/products/price-range?min=300&amp;max=500
    ///     
    /// Response: მითითებულ ფასის დიაპაზონში მყოფი პროდუქტები
    /// </remarks>
    [HttpGet("price-range")]
    public async Task<ActionResult<List<Product>>> GetByPriceRange([FromQuery] decimal min, [FromQuery] decimal max)
    {
        if (min < 0 || max < 0 || min > max)
            return BadRequest("Invalid price range");

        var products = await _searchService.GetProductsInPriceRangeAsync(min, max);
        return Ok(products);
    }

    /// <summary>
    /// Get category statistics - კატეგორიების სტატისტიკა (aggregation)
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     GET /api/products/categories/stats
    ///     
    /// Response: { "laptops": 6, "smartphones": 4, "audio": 2 }
    /// </remarks>
    [HttpGet("categories/stats")]
    public async Task<ActionResult<Dictionary<string, long>>> GetCategoryStats()
    {
        var stats = await _searchService.GetProductCountByCategoryAsync();
        return Ok(stats);
    }
}

