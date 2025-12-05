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
    /// Search products by query
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<Product>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query))
            return BadRequest("Query is required");

        var products = await _searchService.SearchProductsAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id)
    {
        var product = await _searchService.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<Product>>> GetByCategory(string category)
    {
        var products = await _searchService.GetProductsByCategoryAsync(category);
        return Ok(products);
    }

    /// <summary>
    /// Get products by price range
    /// </summary>
    [HttpGet("price-range")]
    public async Task<ActionResult<List<Product>>> GetByPriceRange([FromQuery] decimal min, [FromQuery] decimal max)
    {
        if (min < 0 || max < 0 || min > max)
            return BadRequest("Invalid price range");

        var products = await _searchService.GetProductsInPriceRangeAsync(min, max);
        return Ok(products);
    }

    /// <summary>
    /// Get product count by category
    /// </summary>
    [HttpGet("categories/stats")]
    public async Task<ActionResult<Dictionary<string, long>>> GetCategoryStats()
    {
        var stats = await _searchService.GetProductCountByCategoryAsync();
        return Ok(stats);
    }
}

