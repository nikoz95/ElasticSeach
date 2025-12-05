﻿using Nest;
using ElasticSeach.Services;
using ElasticSeach.KibanaGuide;

namespace ElasticSeach;

/// <summary>
/// Elasticsearch Level II Demonstration Project
/// Topics: API, Index Creation, Templates, Mappings, Indexing, Text Analysis, Query DSL, Kibana
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     ELASTICSEARCH LEVEL II - DEMONSTRATION PROJECT          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Get Elasticsearch URL from environment variable (Docker) or use default (local)
        var elasticsearchUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://localhost:9200";
        Console.WriteLine($"🌐 Elasticsearch URL: {elasticsearchUrl}");
        Console.WriteLine();

        // Initialize Elasticsearch client - try both HTTP and HTTPS
        ElasticClient client;
        
        // Try HTTPS first (for Elasticsearch 8.x+)
        try
        {
            Console.WriteLine("Attempting HTTPS connection...");
            var httpsUri = new Uri(elasticsearchUrl.Replace("http://", "https://"));
            var httpsSettings = new ConnectionSettings(httpsUri)
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true) // Accept self-signed certificates
                .BasicAuthentication("elastic", "changeme") // Default credentials, will try without if fails
                .DefaultIndex("products")
                .DisableDirectStreaming()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromSeconds(10));

            client = new ElasticClient(httpsSettings);
            var httpsTest = await client.PingAsync();
            
            if (!httpsTest.IsValid)
            {
                // Try without authentication
                Console.WriteLine("Trying HTTPS without authentication...");
                httpsSettings = new ConnectionSettings(new Uri("https://localhost:9200"))
                    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                    .DefaultIndex("products")
                    .DisableDirectStreaming()
                    .PrettyJson()
                    .RequestTimeout(TimeSpan.FromSeconds(10));
                
                client = new ElasticClient(httpsSettings);
                httpsTest = await client.PingAsync();
                
                if (!httpsTest.IsValid)
                {
                    throw new Exception("HTTPS connection failed");
                }
            }
            Console.WriteLine("✓ HTTPS connection successful");
        }
        catch
        {
            // Fall back to HTTP
            Console.WriteLine("HTTPS failed, trying HTTP...");
            var httpUri = new Uri(elasticsearchUrl);
            var httpSettings = new ConnectionSettings(httpUri)
                .DefaultIndex("products")
                .DisableDirectStreaming()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromMinutes(2));

            client = new ElasticClient(httpSettings);
        }

        // Check connection
        if (!await CheckConnectionAsync(client))
        {
            Console.WriteLine($"\n⚠ Make sure Elasticsearch is running on {elasticsearchUrl}");
            Console.WriteLine("Start Elasticsearch and Kibana, then run this program again.");
            Console.WriteLine("\nPress any key to continue with offline demonstration...");
            Console.ReadKey();
            ShowOfflineDemonstration();
            return;
        }

        // Initialize services
        var indexManagement = new IndexManagementService(client);
        var mappingService = new MappingService(client);
        var documentIndexing = new DocumentIndexingService(client);
        var textAnalysis = new TextAnalysisService(client);
        var searchService = new SearchService(client);

        try
        {
            // Display menu
            while (true)
            {
                Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                      DEMONSTRATION MENU                      ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("  1. Index Management (Creating Index and Templates)");
                Console.WriteLine("  2. Data Types and Mappings");
                Console.WriteLine("  3. Indexing Documents");
                Console.WriteLine("  4. Text Analysis Basics");
                Console.WriteLine("  5. Query DSL Basics");
                Console.WriteLine("  6. Advanced Search Queries");
                Console.WriteLine("  7. Kibana Commands Guide");
                Console.WriteLine("  8. Run Complete Demonstration");
                Console.WriteLine("  9. List All Indexes");
                Console.WriteLine("  0. Exit");
                Console.WriteLine();
                Console.Write("Select option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await RunIndexManagementDemo(indexManagement);
                        break;
                    case "2":
                        await RunMappingDemo(mappingService);
                        break;
                    case "3":
                        await RunDocumentIndexingDemo(documentIndexing);
                        break;
                    case "4":
                        await RunTextAnalysisDemo(textAnalysis);
                        break;
                    case "5":
                        await RunBasicSearchDemo(searchService);
                        break;
                    case "6":
                        await RunAdvancedSearchDemo(searchService);
                        break;
                    case "7":
                        ShowKibanaGuide();
                        break;
                    case "8":
                        await RunCompleteDemo(indexManagement, mappingService, documentIndexing, 
                            textAnalysis, searchService);
                        break;
                    case "9":
                        await ListAllIndexes(client);
                        break;
                    case "0":
                        Console.WriteLine("\nThank you for using the Elasticsearch Demo!");
                        return;
                    default:
                        Console.WriteLine("\n✗ Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\n\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static async Task<bool> CheckConnectionAsync(ElasticClient client)
    {
        try
        {
            Console.WriteLine("Checking Elasticsearch connection...");
            var pingResponse = await client.PingAsync();
            
            if (pingResponse.IsValid)
            {
                var info = await client.RootNodeInfoAsync();
                Console.WriteLine($"✓ Connected to Elasticsearch {info.Version.Number}");
                Console.WriteLine($"  Cluster: {info.ClusterName}");
                return true;
            }
            else
            {
                Console.WriteLine($"✗ Connection failed: {pingResponse.DebugInformation}");
                
                // Check if it's a security issue
                if (pingResponse.DebugInformation.Contains("unable to verify") || 
                    pingResponse.DebugInformation.Contains("unsuccessful product check") ||
                    pingResponse.DebugInformation.Contains("response ended prematurely"))
                {
                    Console.WriteLine("\n⚠️  SECURITY IS ENABLED ON ELASTICSEARCH");
                    Console.WriteLine("\n🔧 Quick Fix:");
                    Console.WriteLine("1. Stop Elasticsearch (Ctrl+C)");
                    Console.WriteLine("2. Edit: config\\elasticsearch.yml");
                    Console.WriteLine("3. Add these lines at the end:");
                    Console.WriteLine("   xpack.security.enabled: false");
                    Console.WriteLine("   xpack.security.http.ssl.enabled: false");
                    Console.WriteLine("4. Save and restart Elasticsearch");
                    Console.WriteLine("5. Run this demo again");
                    Console.WriteLine("\n📄 See FIX_SECURITY.md for detailed instructions");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Connection error: {ex.Message}");
            return false;
        }
    }

    static async Task RunIndexManagementDemo(IndexManagementService service)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 1: INDEX MANAGEMENT & TEMPLATES");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        await service.CreateProductIndexAsync();
        await service.CreateArticleIndexAsync();
        await service.CreateIndexTemplateAsync();
        await service.DisplayIndexInfoAsync("products");
    }

    static async Task RunMappingDemo(MappingService service)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 2: DATA TYPES AND MAPPINGS");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        // Create proper indexes with mappings first
        await service.CreateProductMappingAsync();
        await service.CreateArticleMappingAsync();
        
        // Then demonstrate data types
        await service.DemonstrateDataTypesAsync();
        await service.ShowMappingAsync("products");
    }

    static async Task RunDocumentIndexingDemo(DocumentIndexingService service)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 3: INDEXING DOCUMENTS");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        await service.IndexSingleDocumentAsync();
        await service.BulkIndexDocumentsAsync();
        await service.UpdateDocumentAsync();
        await service.IndexArticlesAsync();
    }

    static async Task RunTextAnalysisDemo(TextAnalysisService service)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 4: TEXT ANALYSIS BASICS");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        await service.DemonstrateAnalyzersAsync();
        await service.DemonstrateTokenizersAsync();
        await service.DemonstrateFiltersAsync();
    }

    static async Task RunBasicSearchDemo(SearchService service)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 5: QUERY DSL BASICS");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        await service.MatchAllQueryAsync();
        await service.MatchQueryAsync();
        await service.MultiMatchQueryAsync();
        await service.TermQueryAsync();
        await service.TermsQueryAsync();
        await service.RangeQueryAsync();
        await service.BoolQueryAsync();
    }

    static async Task RunAdvancedSearchDemo(SearchService service)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 6: ADVANCED SEARCH & AGGREGATIONS");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        await service.WildcardAndFuzzyQueriesAsync();
        await service.AggregationsAsync();
        await service.SortingAndPaginationAsync();
        await service.HighlightingAsync();
        await service.SearchArticlesAsync();
    }

    static void ShowKibanaGuide()
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   TOPIC 7: KIBANA DEV TOOLS & DISCOVER");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("KIBANA ACCESS:");
        Console.WriteLine("  URL: http://localhost:5601");
        Console.WriteLine("  Dev Tools: Management > Dev Tools");
        Console.WriteLine("  Discover: Analytics > Discover");
        Console.WriteLine();
        Console.WriteLine("Copy the commands from KibanaCommands.cs file");
        Console.WriteLine("and paste them into Kibana Dev Tools Console.");
        Console.WriteLine();
        Console.WriteLine("Key features to demonstrate:");
        Console.WriteLine("  • Dev Tools Console - Execute Elasticsearch queries");
        Console.WriteLine("  • Discover - Interactive data exploration");
        Console.WriteLine("  • Index Patterns - Configure data views");
        Console.WriteLine("  • KQL (Kibana Query Language) - Simple query syntax");
        Console.WriteLine();
        
        Console.WriteLine("\n--- SAMPLE KIBANA COMMANDS ---\n");
        Console.WriteLine(KibanaCommands.Guide.Substring(0, Math.Min(2000, KibanaCommands.Guide.Length)));
        Console.WriteLine("\n... (See KibanaCommands.cs for full guide)");
    }

    static async Task ListAllIndexes(ElasticClient client)
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   ALL ELASTICSEARCH INDEXES");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        try
        {
            var catResponse = await client.Cat.IndicesAsync(i => i
                .Headers("index,docs.count,store.size,health,status")
            );

            if (catResponse.IsValid)
            {
                Console.WriteLine("Index Name                                    Documents    Size       Health  Status");
                Console.WriteLine("────────────────────────────────────────────────────────────────────────────────────");
                
                foreach (var record in catResponse.Records)
                {
                    // Skip system indexes that start with .
                    if (!record.Index.StartsWith("."))
                    {
                        Console.WriteLine($"{record.Index,-45} {record.DocsCount,10} {record.StoreSize,10}  {record.Health,6}  {record.Status}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("To see system indexes, use: GET _cat/indices?v in Kibana Dev Tools");
            }
            else
            {
                Console.WriteLine($"✗ Error retrieving indexes: {catResponse.DebugInformation}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task RunCompleteDemo(IndexManagementService indexManagement, 
        MappingService mappingService, 
        DocumentIndexingService documentIndexing,
        TextAnalysisService textAnalysis,
        SearchService searchService)
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           COMPLETE ELASTICSEARCH DEMONSTRATION               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Running all demonstrations in sequence...\n");

        // 1. Index Management
        await RunIndexManagementDemo(indexManagement);
        await Task.Delay(1000);

        // 2. Mappings
        await RunMappingDemo(mappingService);
        await Task.Delay(1000);

        // 3. Document Indexing
        await RunDocumentIndexingDemo(documentIndexing);
        await Task.Delay(1000);

        // 4. Text Analysis
        await RunTextAnalysisDemo(textAnalysis);
        await Task.Delay(1000);

        // 5. Basic Search
        await RunBasicSearchDemo(searchService);
        await Task.Delay(1000);

        // 6. Advanced Search
        await RunAdvancedSearchDemo(searchService);

        Console.WriteLine("\n\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              DEMONSTRATION COMPLETED!                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Summary of demonstrated topics:");
        Console.WriteLine("  ✓ 1. Creating Index and Templates");
        Console.WriteLine("  ✓ 2. Data Types and Mappings");
        Console.WriteLine("  ✓ 3. Indexing Documents (Single, Bulk, Update)");
        Console.WriteLine("  ✓ 4. Text Analysis (Analyzers, Tokenizers, Filters)");
        Console.WriteLine("  ✓ 5. Query DSL Basics (Match, Term, Range, Bool)");
        Console.WriteLine("  ✓ 6. Advanced Search (Aggregations, Sorting, Highlighting)");
        Console.WriteLine("  ✓ 7. NEST and Elasticsearch.Net usage in .NET");
        Console.WriteLine();
        Console.WriteLine("Next step: Open Kibana (http://localhost:5601) to explore the data!");
    }

    static void ShowOfflineDemonstration()
    {
        Console.WriteLine("\n\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              OFFLINE DEMONSTRATION MODE                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("This project demonstrates the following Elasticsearch concepts:");
        Console.WriteLine();
        Console.WriteLine("1. ELASTICSEARCH API & CONNECTION");
        Console.WriteLine("   - NEST client configuration");
        Console.WriteLine("   - Connection settings and authentication");
        Console.WriteLine();
        Console.WriteLine("2. CREATING INDEX AND TEMPLATES");
        Console.WriteLine("   - Index creation with settings (shards, replicas)");
        Console.WriteLine("   - Custom analyzers configuration");
        Console.WriteLine("   - Index templates for pattern matching");
        Console.WriteLine();
        Console.WriteLine("3. DATA TYPES AND MAPPINGS");
        Console.WriteLine("   - Text, Keyword, Date, Numeric, Boolean");
        Console.WriteLine("   - Object and Nested types");
        Console.WriteLine("   - Geo-point and IP types");
        Console.WriteLine("   - Explicit vs. Dynamic mapping");
        Console.WriteLine();
        Console.WriteLine("4. INDEXING DOCUMENTS");
        Console.WriteLine("   - Single document indexing");
        Console.WriteLine("   - Bulk indexing for performance");
        Console.WriteLine("   - Update and Delete operations");
        Console.WriteLine();
        Console.WriteLine("5. TEXT ANALYSIS BASICS");
        Console.WriteLine("   - Built-in analyzers (standard, simple, english)");
        Console.WriteLine("   - Custom analyzers");
        Console.WriteLine("   - Tokenizers and Token filters");
        Console.WriteLine("   - Stemming and Stop words");
        Console.WriteLine();
        Console.WriteLine("6. QUERY DSL BASICS");
        Console.WriteLine("   - Match and Multi-match queries");
        Console.WriteLine("   - Term and Terms queries");
        Console.WriteLine("   - Range queries");
        Console.WriteLine("   - Bool queries (must, should, filter)");
        Console.WriteLine("   - Wildcard and Fuzzy queries");
        Console.WriteLine();
        Console.WriteLine("7. ADVANCED SEARCH");
        Console.WriteLine("   - Aggregations (terms, stats, histogram)");
        Console.WriteLine("   - Sorting and Pagination");
        Console.WriteLine("   - Highlighting search results");
        Console.WriteLine();
        Console.WriteLine("8. KIBANA BASICS");
        Console.WriteLine("   - Dev Tools Console commands");
        Console.WriteLine("   - Discover for data exploration");
        Console.WriteLine("   - Index pattern configuration");
        Console.WriteLine();
        Console.WriteLine("Review the code in Services/ folder to see implementations!");
    }
}
