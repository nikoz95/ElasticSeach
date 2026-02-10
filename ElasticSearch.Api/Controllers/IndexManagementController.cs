using Microsoft.AspNetCore.Mvc;
using ElasticSearch.Core.Services;

namespace ElasticSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IndexManagementController : ControllerBase
{
    private readonly IndexMappingService _mappingService;

    public IndexManagementController(IndexMappingService mappingService)
    {
        _mappingService = mappingService;
    }

    /// <summary>
    /// Get index mapping - ინდექსის სტრუქტურის ნახვა
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     GET /api/indexmanagement/mapping/products
    ///     
    /// Response: ინდექსის სრული mapping (fields, types, analyzers)
    /// </remarks>
    [HttpGet("mapping/{indexName}")]
    public async Task<ActionResult<object>> GetMapping(string indexName)
    {
        var mapping = await _mappingService.GetIndexMappingAsync(indexName);
        return Ok(new { Index = indexName, Mapping = mapping });
    }

    /// <summary>
    /// Recreate index - ინდექსის თავიდან შექმნა (ძველი წაიშლება)
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/indexmanagement/recreate?indexName=products
    ///     
    /// გაფრთხილება: წაიშლება ძველი ინდექსი და შეიქმნება ახალი
    /// </remarks>
    [HttpPost("recreate")]
    public async Task<ActionResult<object>> RecreateIndex([FromQuery] string indexName = "products")
    {
        var result = await _mappingService.CreateProductIndexWithMappingsAsync(indexName);
        return result
            ? Ok(new { Success = true, Message = $"Index '{indexName}' recreated successfully" })
            : BadRequest(new { Success = false, Message = "Failed to recreate index" });
    }

    /// <summary>
    /// Test analyzer - analyzer-ის ტეს��ირება (როგორ დაიშლება ტექსტი tokens-ად)
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/indexmanagement/test-analyzer?text=MacBook Pro 16&amp;analyzer=standard
    ///     
    /// Response: ["macbook", "pro", "16"] - tokens after analysis
    /// </remarks>
    [HttpPost("test-analyzer")]
    public async Task<ActionResult<object>> TestAnalyzer(
        [FromQuery] string text,
        [FromQuery] string analyzer = "standard")
    {
        var tokens = await _mappingService.TestAnalyzerAsync(text, analyzer);
        return Ok(new { Text = text, Analyzer = analyzer, Tokens = tokens });
    }

    /// <summary>
    /// Reindex data - მონაცემების გადატანა ერთი ინდექსიდან მეორეში
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/indexmanagement/reindex?sourceIndex=products-old&amp;destIndex=products-new
    ///     
    /// გამოიყენება ინდექსის განახლებისას zero-downtime migration-სთვის
    /// </remarks>
    [HttpPost("reindex")]
    public async Task<ActionResult<object>> Reindex(
        [FromQuery] string sourceIndex,
        [FromQuery] string destIndex)
    {
        var result = await _mappingService.ReindexAsync(sourceIndex, destIndex);
        return result
            ? Ok(new { Success = true, Message = $"Reindexed from '{sourceIndex}' to '{destIndex}'" })
            : BadRequest(new { Success = false, Message = "Reindex failed" });
    }
}

