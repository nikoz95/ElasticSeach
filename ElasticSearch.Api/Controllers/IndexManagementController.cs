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
    /// Create index with custom mappings and analyzers
    /// </summary>
    [HttpPost("create-advanced")]
    public async Task<ActionResult<object>> CreateAdvancedIndex([FromQuery] string indexName = "products-v2")
    {
        var result = await _mappingService.CreateProductIndexWithMappingsAsync(indexName);
        return result
            ? Ok(new { Success = true, Message = $"Index '{indexName}' created successfully" })
            : BadRequest(new { Success = false, Message = "Failed to create index" });
    }

    /// <summary>
    /// Create index template for all indices matching pattern
    /// </summary>
    [HttpPost("create-template")]
    public async Task<ActionResult<object>> CreateIndexTemplate()
    {
        var result = await _mappingService.CreateProductIndexTemplateAsync();
        return result
            ? Ok(new { Success = true, Message = "Index template created successfully" })
            : BadRequest(new { Success = false, Message = "Failed to create template" });
    }

    /// <summary>
    /// Test analyzer with sample text
    /// </summary>
    [HttpPost("test-analyzer")]
    public async Task<ActionResult<object>> TestAnalyzer(
        [FromQuery] string text,
        [FromQuery] string analyzer = "standard")
    {
        var tokens = await _mappingService.TestAnalyzerAsync(text, analyzer);
        return Ok(new { Text = text, Analyzer = analyzer, Tokens = tokens });
    }

    /// <summary>
    /// Get mapping for an index
    /// </summary>
    [HttpGet("mapping/{indexName}")]
    public async Task<ActionResult<object>> GetMapping(string indexName)
    {
        var mapping = await _mappingService.GetIndexMappingAsync(indexName);
        return Ok(new { Index = indexName, Mapping = mapping });
    }

    /// <summary>
    /// Reindex data from one index to another
    /// </summary>
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

    /// <summary>
    /// Create index with alias (for zero-downtime reindexing)
    /// </summary>
    [HttpPost("create-with-alias")]
    public async Task<ActionResult<object>> CreateIndexWithAlias(
        [FromQuery] string indexName,
        [FromQuery] string aliasName)
    {
        var result = await _mappingService.CreateIndexWithAliasAsync(indexName, aliasName);
        return result
            ? Ok(new { Success = true, Message = $"Index '{indexName}' created with alias '{aliasName}'" })
            : BadRequest(new { Success = false, Message = "Failed to create index with alias" });
    }

    /// <summary>
    /// Create demo index with all data types
    /// </summary>
    [HttpPost("create-demo-data-types")]
    public async Task<ActionResult<object>> CreateDemoDataTypes()
    {
        var result = await _mappingService.CreateIndexWithAllDataTypesAsync();
        return result
            ? Ok(new { Success = true, Message = "Demo index with all data types created" })
            : BadRequest(new { Success = false, Message = "Failed to create demo index" });
    }
}

