using Nest;

namespace ElasticSeach.Services;

/// <summary>
/// 5. Text Analysis Basics
/// Demonstrates analyzers, tokenizers, and filters
/// </summary>
public class TextAnalysisService
{
    private readonly ElasticClient _client;

    public TextAnalysisService(ElasticClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Analyze text with different analyzers
    /// </summary>
    public async Task DemonstrateAnalyzersAsync()
    {
        Console.WriteLine("\n=== 5. Text Analysis Basics ===");

        var text = "The Quick Brown Foxes are jumping over the lazy dogs!";

        // Standard Analyzer
        Console.WriteLine("\n5.1. Standard Analyzer (default)");
        await AnalyzeText("products", "standard", text);

        // Simple Analyzer
        Console.WriteLine("\n5.2. Simple Analyzer");
        await AnalyzeText("products", "simple", text);

        // Whitespace Analyzer
        Console.WriteLine("\n5.3. Whitespace Analyzer");
        await AnalyzeText("products", "whitespace", text);

        // English Analyzer
        Console.WriteLine("\n5.4. English Analyzer (with stemming)");
        await AnalyzeText("articles", "english_analyzer", text);

        // Custom Analyzer
        Console.WriteLine("\n5.5. Custom Product Analyzer");
        await AnalyzeText("products", "product_analyzer", text);
    }

    private async Task AnalyzeText(string index, string analyzer, string text)
    {
        var analyzeResponse = await _client.Indices.AnalyzeAsync(a => a
            .Index(index)
            .Analyzer(analyzer)
            .Text(text)
        );

        if (analyzeResponse.IsValid)
        {
            Console.WriteLine($"Input: \"{text}\"");
            Console.WriteLine("Tokens:");
            foreach (var token in analyzeResponse.Tokens)
            {
                Console.WriteLine($"  • [{token.Position}] '{token.Token}' (type: {token.Type})");
            }
            Console.WriteLine($"Total tokens: {analyzeResponse.Tokens.Count}");
        }
    }

    /// <summary>
    /// Demonstrate tokenizers
    /// </summary>
    public async Task DemonstrateTokenizersAsync()
    {
        Console.WriteLine("\n=== Text Analysis: Tokenizers ===");

        var text = "john.doe@example.com visits https://elasticsearch.org";

        // Standard Tokenizer
        Console.WriteLine("\nStandard Tokenizer:");
        await AnalyzeWithTokenizer("standard", text);

        // Keyword Tokenizer (no tokenization)
        Console.WriteLine("\nKeyword Tokenizer:");
        await AnalyzeWithTokenizer("keyword", text);

        // Pattern Tokenizer
        Console.WriteLine("\nPattern Tokenizer (split on whitespace):");
        await AnalyzeWithTokenizer("pattern", text);
    }

    private async Task AnalyzeWithTokenizer(string tokenizer, string text)
    {
        var analyzeResponse = await _client.Indices.AnalyzeAsync(a => a
            .Tokenizer(tokenizer)
            .Text(text)
        );

        if (analyzeResponse.IsValid)
        {
            Console.WriteLine($"Input: \"{text}\"");
            Console.WriteLine("Tokens: " + string.Join(", ", analyzeResponse.Tokens.Select(t => $"'{t.Token}'")));
        }
    }

    /// <summary>
    /// Demonstrate token filters
    /// </summary>
    public async Task DemonstrateFiltersAsync()
    {
        Console.WriteLine("\n=== Text Analysis: Token Filters ===");

        var text = "The QUICK Brown foxes are RUNNING!";

        // Lowercase filter
        Console.WriteLine("\nLowercase Filter:");
        await AnalyzeWithFilters("standard", new[] { "lowercase" }, text);

        // Stemmer filter
        Console.WriteLine("\nStemmer Filter (English):");
        await AnalyzeWithFilters("standard", new[] { "lowercase", "porter_stem" }, text);

        // Stop words filter
        Console.WriteLine("\nStop Words Filter:");
        await AnalyzeWithFilters("standard", new[] { "lowercase", "stop" }, text);
    }

    private async Task AnalyzeWithFilters(string tokenizer, string[] filters, string text)
    {
        var analyzeResponse = await _client.Indices.AnalyzeAsync(a => a
            .Tokenizer(tokenizer)
            .Filter(filters)
            .Text(text)
        );

        if (analyzeResponse.IsValid)
        {
            Console.WriteLine($"Input: \"{text}\"");
            Console.WriteLine("Tokens: " + string.Join(", ", analyzeResponse.Tokens.Select(t => $"'{t.Token}'")));
        }
    }

    /// <summary>
    /// Create index with custom analyzers
    /// </summary>
    public async Task CreateIndexWithCustomAnalyzersAsync()
    {
        Console.WriteLine("\n=== Creating Index with Custom Analyzers ===");

        var indexName = "custom-analysis";
        
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            await _client.Indices.DeleteAsync(indexName);
        }

        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    // Custom Analyzers
                    .Analyzers(an => an
                        .Custom("my_custom_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "stop", "snowball")
                        )
                        .Custom("email_analyzer", ca => ca
                            .Tokenizer("uax_url_email")
                            .Filters("lowercase", "unique")
                        )
                    )
                    // Custom Token Filters
                    .TokenFilters(tf => tf
                        .Stemmer("my_stemmer", st => st
                            .Language("english")
                        )
                        .Stop("my_stop_filter", st => st
                            .StopWords("a", "an", "the", "is", "are")
                        )
                    )
                    // Custom Tokenizers
                    .Tokenizers(t => t
                        .Pattern("my_pattern_tokenizer", pt => pt
                            .Pattern(@"\W+") // Split on non-word characters
                        )
                    )
                )
            )
        );

        if (createResponse.IsValid)
        {
            Console.WriteLine("✓ Index with custom analyzers created!");
            Console.WriteLine("Custom analyzers available:");
            Console.WriteLine("  • my_custom_analyzer - standard + lowercase + stop + snowball");
            Console.WriteLine("  • email_analyzer - for analyzing email addresses");
        }
    }
}

