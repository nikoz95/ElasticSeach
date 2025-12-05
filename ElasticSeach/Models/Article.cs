namespace ElasticSeach.Models;

/// <summary>
/// Article document model - for demonstrating text analysis
/// </summary>
public class Article
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public List<string> Keywords { get; set; } = new();
    public int ViewCount { get; set; }
}

