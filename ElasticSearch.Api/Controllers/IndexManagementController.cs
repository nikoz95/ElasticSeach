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

    /// <summary>
    /// Create Index Template - ინდექსის შაბლონის შექმნა
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/indexmanagement/template?templateName=products_template
    ///     
    /// შაბლონი ავტომატურად გაავრცელებს settings და mappings ნებისმიერ ახალ ინდექსზე, რომლის სახელი იწყება "products-"-ით
    /// </remarks>
    [HttpPost("template")]
    public async Task<ActionResult<object>> CreateTemplate([FromQuery] string templateName = "products_template")
    {
        var result = await _mappingService.CreateProductIndexTemplateAsync(templateName);
        return result
            ? Ok(new { Success = true, Message = $"Index Template '{templateName}' created successfully" })
            : BadRequest(new { Success = false, Message = "Failed to create index template" });
    }

    /// <summary>
    /// Switch Alias - ალიასის გადართვა ძველი ინდექსიდან ახალზე (Zero-Downtime)
    /// </summary>
    [HttpPost("switch-alias")]
    public async Task<ActionResult<object>> SwitchAlias(
        [FromQuery] string aliasName,
        [FromQuery] string oldIndex,
        [FromQuery] string newIndex)
    {
        var result = await _mappingService.UpdateAliasAsync(aliasName, oldIndex, newIndex);
        return result
            ? Ok(new { Success = true, Message = $"Alias '{aliasName}' switched to '{newIndex}'" })
            : BadRequest(new { Success = false, Message = "Failed to switch alias" });
    }

    /// <summary>
    /// Add Alias - ალიასის დამატება ინდექსზე
    /// </summary>
    [HttpPost("add-alias")]
    public async Task<ActionResult<object>> AddAlias(
        [FromQuery] string indexName,
        [FromQuery] string aliasName)
    {
        var result = await _mappingService.AddAliasAsync(indexName, aliasName);
        return result
            ? Ok(new { Success = true, Message = $"Alias '{aliasName}' added to '{indexName}'" })
            : BadRequest(new { Success = false, Message = "Failed to add alias" });
    }

    /// <summary>
    /// Get Alias indices - ალიასთან დაკავშირებული ინდექსების ნახვა
    /// </summary>
    [HttpGet("alias/{aliasName}")]
    public async Task<ActionResult<object>> GetAlias(string aliasName)
    {
        var indices = await _mappingService.GetIndicesForAliasAsync(aliasName);
        return Ok(new { Alias = aliasName, Indices = indices });
    }

    /// <summary>
    /// Shrink index - ამცირებს primary shard-ების რაოდენობას (მარტივი ინდექსებისთვის ოპტიმიზაცია)
    /// </summary>
    /// <remarks>
    /// Example:
    ///     POST /api/indexmanagement/shrink?sourceIndex=products-v2&targetIndex=products-v2-shrunk&targetShards=1
    /// </remarks>
    [HttpPost("shrink")]
    public async Task<ActionResult<object>> Shrink(
        [FromQuery] string sourceIndex,
        [FromQuery] string targetIndex,
        [FromQuery] int targetShards = 1)
    {
        var ok = await _mappingService.ShrinkIndexAsync(sourceIndex, targetIndex, targetShards);
        return ok
            ? Ok(new { Success = true, Message = $"Index '{sourceIndex}' shrunk to '{targetIndex}' ({targetShards} shards)" })
            : BadRequest(new { Success = false, Message = "Shrink failed" });
    }

    /// <summary>
    /// Split index - ზრდის primary shard-ების რაოდენობას (მასშტაბირებისთვის)
    /// </summary>
    /// <remarks>
    /// Example:
    ///     POST /api/indexmanagement/split?sourceIndex=products-v2&targetIndex=products-v2-split&targetShards=6
    /// </remarks>
    [HttpPost("split")]
    public async Task<ActionResult<object>> Split(
        [FromQuery] string sourceIndex,
        [FromQuery] string targetIndex,
        [FromQuery] int targetShards = 6)
    {
        var ok = await _mappingService.SplitIndexAsync(sourceIndex, targetIndex, targetShards);
        return ok
            ? Ok(new { Success = true, Message = $"Index '{sourceIndex}' split to '{targetIndex}' ({targetShards} shards)" })
            : BadRequest(new { Success = false, Message = "Split failed" });
    }

    /// <summary>
    /// Clone index - ინდექსის ზუსტი ასლის შექმნა
    /// </summary>
    /// <remarks>
    /// Example:
    ///     POST /api/indexmanagement/clone?sourceIndex=products-v2&targetIndex=products-v2-clone
    /// </remarks>
    [HttpPost("clone")]
    public async Task<ActionResult<object>> Clone(
        [FromQuery] string sourceIndex,
        [FromQuery] string targetIndex)
    {
        var ok = await _mappingService.CloneIndexAsync(sourceIndex, targetIndex);
        return ok
            ? Ok(new { Success = true, Message = $"Index '{sourceIndex}' cloned to '{targetIndex}'" })
            : BadRequest(new { Success = false, Message = "Clone failed" });
    }

    /// <summary>
    /// Force merge - სეგმენტების ოპტიმიზაცია (გამოიყენეთ მხოლოდ სტატიკურ ინდექსებზე)
    /// </summary>
    /// <remarks>
    /// Example:
    ///     POST /api/indexmanagement/forcemerge?indexName=products-v2&maxSegments=1
    /// </remarks>
    /// <summary>
    /// Bulk update with script - მასობრივი განახლება სკრიპტით (მაგ. ფასდაკლება კატეგორიაზე)
    /// </summary>
    [HttpPost("bulk-update-script")]
    public async Task<ActionResult<object>> BulkUpdateScript(
        [FromQuery] string category,
        [FromQuery] double discount)
    {
        var result = await _mappingService.BulkUpdateWithScriptAsync(category, discount);
        return result
            ? Ok(new { Success = true, Message = $"Successfully applied {discount * 100}% discount to '{category}'" })
            : BadRequest(new { Success = false, Message = "Bulk update failed" });
    }
}

