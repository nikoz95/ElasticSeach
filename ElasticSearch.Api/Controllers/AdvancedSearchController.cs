using Microsoft.AspNetCore.Mvc;
using ElasticSearch.Core.Models;
using ElasticSearch.Core.Services;

namespace ElasticSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdvancedSearchController(AdvancedSearchService advancedSearchService) : ControllerBase
{
    /// <summary>
    /// Complex bool query with filters - ყველაზე ხშირად გამოსაყენებელი advanced search
    /// </summary>
    /// <remarks>
    /// Example requests:
    /// 
    ///     GET /api/advancedsearch/complex?query=macbook&amp;category=laptops&amp;maxPrice=3000
    ///     GET /api/advancedsearch/complex?query=iphone&amp;category=smartphones
    ///     GET /api/advancedsearch/complex?query=wireless&amp;maxPrice=500
    ///     
    /// Response: ფილტრებით დალაგებული პროდუქტების სია
    /// </remarks>
    [HttpGet("complex")]
    public async Task<ActionResult<List<Product>>> ComplexSearch(
        [FromQuery] string query,
        [FromQuery] string? category = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var products = await advancedSearchService.ComplexBoolSearchAsync(query, category, maxPrice, page, pageSize);
        return Ok(products);
    }

    /// <summary>
    /// Fuzzy search - მომხმარებლის შეცდომების ტოლერანტი ძებნა
    /// </summary>
    /// <remarks>
    /// Example requests:
    /// 
    ///     GET /api/advancedsearch/fuzzy?query=mackbok
    ///     GET /api/advancedsearch/fuzzy?query=samsng
    ///     GET /api/advancedsearch/fuzzy?query=iphne
    ///     
    /// აბრუნებს: "macbook", "samsung", "iphone" პროდუქტებს, მიუხედავად typo-ებისა
    /// </remarks>
    [HttpGet("fuzzy")]
    public async Task<ActionResult<List<Product>>> FuzzySearch([FromQuery] string query, [FromQuery] int maxEdits = 2)
    {
        var products = await advancedSearchService.FuzzySearchAsync(query, maxEdits);
        return Ok(products);
    }

    /// <summary>
    /// Autocomplete - პრეფიქსის მიხედვით ძებნა (autocomplete ფუნქციონალისთვის)
    /// </summary>
    /// <remarks>
    /// Example requests:
    /// 
    ///     GET /api/advancedsearch/autocomplete?prefix=mac
    ///     GET /api/advancedsearch/autocomplete?prefix=iph
    ///     GET /api/advancedsearch/autocomplete?prefix=dell
    ///     
    /// აბრუნებს: პროდუქტებს რომლებიც იწყება მითითებული პრეფიქსით
    /// </remarks>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<Product>>> AutocompleteSearch([FromQuery] string prefix, [FromQuery] int limit = 10)
    {
        var products = await advancedSearchService.PrefixSearchAsync(prefix, limit);
        return Ok(products);
    }

    /// <summary>
    /// Paginated search - pagination-ით ძებნა და total count
    /// </summary>
    /// <remarks>
    /// Example requests:
    /// 
    ///     GET /api/advancedsearch/paginated?query=macbook&amp;page=1&amp;pageSize=10
    ///     GET /api/advancedsearch/paginated?query=samsung&amp;page=2&amp;pageSize=5
    ///     
    /// Response: { "page": 1, "pageSize": 10, "total": 25, "totalPages": 3, "products": [...] }
    /// </remarks>
    [HttpGet("paginated1")]
    public async Task<ActionResult<object>> PaginatedSearch(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (products, total) = await advancedSearchService.PaginatedSearchAsync(query, page, pageSize);
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
