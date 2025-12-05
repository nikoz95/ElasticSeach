using Microsoft.AspNetCore.Mvc;
using ElasticSearch.Core.Models;
using ElasticSearch.Core.Services;

namespace ElasticSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdvancedSearchController : ControllerBase
{
    private readonly AdvancedSearchService _advancedSearchService;

    public AdvancedSearchController(AdvancedSearchService advancedSearchService)
    {
        _advancedSearchService = advancedSearchService;
    }

    /// <summary>
    /// Complex bool query with must + filter + should
    /// </summary>
    [HttpGet("complex")]
    public async Task<ActionResult<List<Product>>> ComplexSearch(
        [FromQuery] string query,
        [FromQuery] string? category = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var products = await _advancedSearchService.ComplexBoolSearchAsync(query, category, maxPrice, page, pageSize);
        return Ok(products);
    }

    /// <summary>
    /// Fuzzy search - tolerates typos (e.g., "laptap" finds "laptop")
    /// </summary>
    [HttpGet("fuzzy")]
    public async Task<ActionResult<List<Product>>> FuzzySearch([FromQuery] string query, [FromQuery] int maxEdits = 2)
    {
        var products = await _advancedSearchService.FuzzySearchAsync(query, maxEdits);
        return Ok(products);
    }

    /// <summary>
    /// Wildcard search - pattern matching (* = any chars)
    /// </summary>
    [HttpGet("wildcard")]
    public async Task<ActionResult<List<Product>>> WildcardSearch([FromQuery] string pattern)
    {
        var products = await _advancedSearchService.WildcardSearchAsync(pattern);
        return Ok(products);
    }

    /// <summary>
    /// Prefix search - autocomplete functionality
    /// </summary>
    [HttpGet("prefix")]
    public async Task<ActionResult<List<Product>>> PrefixSearch([FromQuery] string prefix, [FromQuery] int limit = 10)
    {
        var products = await _advancedSearchService.PrefixSearchAsync(prefix, limit);
        return Ok(products);
    }

    /// <summary>
    /// Regexp search - regular expression matching
    /// </summary>
    [HttpGet("regexp")]
    public async Task<ActionResult<List<Product>>> RegexpSearch([FromQuery] string pattern)
    {
        var products = await _advancedSearchService.RegexpSearchAsync(pattern);
        return Ok(products);
    }

    /// <summary>
    /// Find products that have (or don't have) a specific field
    /// </summary>
    [HttpGet("exists")]
    public async Task<ActionResult<List<Product>>> ExistsQuery([FromQuery] string fieldName, [FromQuery] bool mustExist = true)
    {
        var products = await _advancedSearchService.ExistsQueryAsync(fieldName, mustExist);
        return Ok(products);
    }

    /// <summary>
    /// Multi-field search (text + keyword fields)
    /// </summary>
    [HttpGet("multi-field")]
    public async Task<ActionResult<List<Product>>> MultiFieldSearch([FromQuery] string query)
    {
        var products = await _advancedSearchService.MultiFieldSearchAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Function score query - custom scoring based on business logic
    /// </summary>
    [HttpGet("function-score")]
    public async Task<ActionResult<List<Product>>> FunctionScoreSearch([FromQuery] string query)
    {
        var products = await _advancedSearchService.FunctionScoreSearchAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Get search suggestions (did you mean?)
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetSuggestions([FromQuery] string query)
    {
        var suggestions = await _advancedSearchService.GetSuggestionsAsync(query);
        return Ok(suggestions);
    }

    /// <summary>
    /// Paginated search with total count
    /// </summary>
    [HttpGet("paginated")]
    public async Task<ActionResult<object>> PaginatedSearch(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (products, total) = await _advancedSearchService.PaginatedSearchAsync(query, page, pageSize);
        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize),
            Products = products
        });
    }
}

