# Elasticsearch Level II - პრეზენტაციის სტრუქტურა

## 🎯 პრეზენტაციის მიზანი
დემონსტრირება Elasticsearch-ის მთავარი ფუნქციების და NEST გამოყენება .NET-ში

---

## SLIDE 1: Title Slide
**Elasticsearch Level II**
**Advanced Search and Analytics**

- თქვენი სახელი
- თარიღი
- .NET + Elasticsearch + Kibana

---

## SLIDE 2: დღის Agenda
### რას განვიხილავთ?

1. ✅ Elasticsearch API და NEST კლიენტი
2. 📊 Index-ების შექმნა და Templates
3. 🗂️ Data Types და Mappings
4. 📝 დოკუმენტების Indexing
5. 🔍 Text Analysis საფუძვლები
6. 🎯 Query DSL და Search
7. 📈 Kibana Tools
8. 💻 NEST გამოყენება .NET-ში

---

## SLIDE 3: რა არის Elasticsearch?

### Distributed Search & Analytics Engine
- 🚀 **Real-time** - მყისიერი ძებნა
- 📊 **Scalable** - ჰორიზონტალური მასშტაბირება
- 🔍 **Full-text** - ძლიერი ტექსტური ძებნა
- 📈 **Analytics** - aggregations და statistics

### Use Cases:
- ლოგების ანალიზი
- E-commerce ძებნა
- Monitoring და Metrics
- Document Management

---

## SLIDE 4: Elasticsearch Architecture

```
┌─────────────────────────────────────┐
│         Application Layer           │
│      (.NET + NEST Client)           │
└──────────────┬──────────────────────┘
               │ HTTP/REST API
┌──────────────┼──────────────────────┐
│         Elasticsearch Cluster       │
│  ┌──────┐  ┌──────┐  ┌──────┐      │
│  │ Node │  │ Node │  │ Node │      │
│  └───┬──┘  └───┬──┘  └───┬──┘      │
│      │ Index   │ Index   │          │
│  [Shard 1] [Shard 2] [Shard 3]     │
└─────────────────────────────────────┘
```

---

## SLIDE 5: NEST კლიენტის Setup

### Connection Configuration
```csharp
var settings = new ConnectionSettings(
    new Uri("http://localhost:9200")
)
.DefaultIndex("products")
.PrettyJson()
.RequestTimeout(TimeSpan.FromMinutes(2));

var client = new ElasticClient(settings);
```

### Connection Testing
```csharp
var pingResponse = await client.PingAsync();
var info = await client.InfoAsync();
```

**🎯 DEMO**: Program.cs - Connection testing

---

## SLIDE 6: Index Management

### რა არის Index?
- მონაცემთა კონტეინერი (მსგავსია Database-ის)
- შეიცავს documents-ს (მსგავსია rows-ის)
- აქვს settings და mappings

### Index Settings:
- **Shards** - მონაცემების დაყოფა
- **Replicas** - backup კოპიები
- **Analyzers** - ტექსტის დამუშავება

**🎯 DEMO**: IndexManagementService.cs
- CreateProductIndexAsync()
- Custom Analyzers
- Index Templates

---

## SLIDE 7: Index Templates

### რატომ Templates?
- ავტომატური configuration რამდენიმე index-სთვის
- Pattern matching (მაგ: `logs-*`)
- Consistent settings

```json
{
  "index_patterns": ["logs-*"],
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 0
  },
  "mappings": {
    "properties": {
      "timestamp": { "type": "date" },
      "message": { "type": "text" }
    }
  }
}
```

**🎯 DEMO**: CreateIndexTemplateAsync()

---

## SLIDE 8: Data Types Overview

### Elasticsearch Data Types:

| Type | Description | Use Case |
|------|-------------|----------|
| **text** | Full-text search | სტატიები, აღწერები |
| **keyword** | Exact matching | კატეგორიები, tags |
| **date** | Date/Time | თარიღები |
| **numeric** | Numbers | ფასები, რაოდენობა |
| **boolean** | true/false | status flags |
| **object** | JSON objects | nested structures |
| **nested** | Array of objects | დამოუკიდებელი querying |
| **geo_point** | Coordinates | მდებარეობა |

**🎯 DEMO**: MappingService.cs

---

## SLIDE 9: Mappings - Field Configuration

### Explicit Mapping
```csharp
.Properties(p => p
    .Text(t => t
        .Name(n => n.Name)
        .Analyzer("standard")
    )
    .Keyword(k => k
        .Name(n => n.Category)
    )
    .Number(n => n
        .Name(nn => nn.Price)
        .Type(NumberType.ScaledFloat)
    )
)
```

### Multi-field Mapping
```csharp
.Text(t => t
    .Name(n => n.Title)
    .Fields(f => f
        .Keyword(k => k.Name("keyword"))
    )
)
```

**🎯 DEMO**: Product model mappings

---

## SLIDE 10: Indexing Documents

### Single Document
```csharp
var indexResponse = await client.IndexAsync(
    product, 
    i => i.Index("products").Id(product.Id)
);
```

### Bulk Indexing
```csharp
var bulkResponse = await client.BulkAsync(b => b
    .Index("products")
    .IndexMany(products)
    .Refresh(Refresh.True)
);
```

### Performance:
- ⚡ Bulk = 10-100x უფრო სწრაფი
- 💰 ნაკლები network overhead

**🎯 DEMO**: DocumentIndexingService.cs

---

## SLIDE 11: Text Analysis Pipeline

### Analysis Flow:
```
Input Text
    ↓
Character Filters (HTML strip, mapping)
    ↓
Tokenizer (split text)
    ↓
Token Filters (lowercase, stemming, stop words)
    ↓
Tokens (indexed)
```

### Example:
```
"The QUICK Brown Foxes are Running!"
    ↓ Standard Analyzer
["quick", "brown", "fox", "run"]
```

**🎯 DEMO**: TextAnalysisService.cs

---

## SLIDE 12: Analyzers

### Built-in Analyzers:
1. **Standard** - ჩვეულებრივი ტექსტი
2. **Simple** - მხოლოდ ასოები
3. **Whitespace** - spaces-ით გაყოფა
4. **English** - stemming + stop words
5. **Keyword** - არ იყოფა

### Custom Analyzer:
```csharp
.Analyzers(an => an
    .Custom("my_analyzer", ca => ca
        .Tokenizer("standard")
        .Filters("lowercase", "stop", "snowball")
    )
)
```

**🎯 DEMO**: Analyzer comparison

---

## SLIDE 13: Query DSL - Match Query

### Full-text Search
```csharp
.Query(q => q
    .Match(m => m
        .Field(f => f.Name)
        .Query("laptop dell")
    )
)
```

### როგორ მუშაობს:
1. Query text გაივლის analysis-ს
2. ხდება tokens-ად დაყოფა
3. ეძებს matching documents-ს
4. გამოითვლება relevance score

**Scoring**: TF-IDF + BM25

**🎯 DEMO**: SearchService.cs - MatchQuery

---

## SLIDE 14: Query DSL - Bool Query

### Combining Queries
```csharp
.Bool(b => b
    .Must(m => ...)        // აუცილებელი (affects score)
    .Filter(f => ...)      // აუცილებელი (no score)
    .Should(s => ...)      // არჩევითი (affects score)
    .MustNot(mn => ...)    // უარყოფა
)
```

### Example: "Electronics < $1000 AND (Apple OR Samsung)"
```csharp
.Bool(b => b
    .Must(m => m.Term(t => t.Field("category").Value("Electronics")))
    .Filter(f => f.Range(r => r.Field("price").LessThan(1000)))
    .Should(
        sh => sh.Match(m => m.Field("name").Query("Apple")),
        sh => sh.Match(m => m.Field("name").Query("Samsung"))
    )
    .MinimumShouldMatch(1)
)
```

**🎯 DEMO**: Bool query examples

---

## SLIDE 15: Query Types Summary

| Query Type | Use Case | Analyzed? |
|------------|----------|-----------|
| **Match** | Full-text search | ✅ |
| **Multi-Match** | Multiple fields | ✅ |
| **Term** | Exact keyword | ❌ |
| **Range** | Numeric/Date ranges | ❌ |
| **Bool** | Complex logic | Mix |
| **Wildcard** | Pattern matching | ❌ |
| **Fuzzy** | Typo tolerance | ✅ |

**Context:**
- Query Context - scoring, relevance
- Filter Context - yes/no, cached

**🎯 DEMO**: Query comparison

---

## SLIDE 16: Aggregations - Analytics

### Types of Aggregations:

1. **Metric** - calculations
   - avg, min, max, sum, stats
   
2. **Bucket** - grouping
   - terms, histogram, date_histogram
   
3. **Pipeline** - aggregations on aggregations

### Example:
```csharp
.Aggregations(a => a
    .Terms("categories", t => t.Field(f => f.Category))
    .Average("avg_price", av => av.Field(f => f.Price))
)
```

**🎯 DEMO**: AggregationsAsync()

---

## SLIDE 17: Advanced Features

### 1. Sorting
```csharp
.Sort(s => s
    .Descending(f => f.Price)
    .Ascending(f => f.Name)
)
```

### 2. Pagination
```csharp
.From(0)
.Size(10)
```

### 3. Highlighting
```csharp
.Highlight(h => h
    .Fields(f => f.Field(ff => ff.Description))
    .PreTags("<mark>")
    .PostTags("</mark>")
)
```

**🎯 DEMO**: Advanced search features

---

## SLIDE 18: Kibana - Dev Tools

### Dev Tools Console
- ✅ REST API testing
- 📝 Query development
- 🔍 Index exploration

### Sample Commands:
```json
GET products/_search
{
  "query": {
    "match": {
      "name": "laptop"
    }
  }
}
```

**🎯 DEMO**: Kibana Dev Tools live
- Index operations
- Search queries
- Analyze API

---

## SLIDE 19: Kibana - Discover

### Interactive Data Exploration
- 📊 Visual data browser
- 🔍 KQL queries
- 📈 Time-based filtering
- 💾 Saved searches

### KQL Examples:
```
category: "Electronics"
price >= 500 and price <= 1500
name: *phone*
tags: (laptop OR tablet)
```

**🎯 DEMO**: Kibana Discover
- Index pattern creation
- Data filtering
- Field analysis

---

## SLIDE 20: NEST Best Practices

### ✅ DO:
- ✔️ გამოიყენე strongly-typed queries
- ✔️ Async/await everywhere
- ✔️ Bulk operations for multiple documents
- ✔️ Connection pooling
- ✔️ Error handling და retries

### ❌ DON'T:
- ✖️ არ გამოიყენო Select * style queries
- ✖️ არ დაივიწყო pagination
- ✖️ არ გაუშვა analyze-ს production-ზე ბევრჯერ
- ✖️ არ შექმნა ბევრი connection

**🎯 Code Examples**

---

## SLIDE 21: Performance Tips

### Indexing Performance:
1. 🚀 Bulk operations (1000-5000 docs)
2. ⚡ Disable refresh during bulk
3. 💾 Increase refresh interval
4. 🔧 Optimize mapping

### Search Performance:
1. 🎯 Filter context როცა შესაძლებელია
2. 💰 Pagination with search_after
3. 📊 Limit fields with _source
4. 🔍 Use routing for specific shards

**🎯 Performance comparison demo**

---

## SLIDE 22: Real-world Use Cases

### 1. E-commerce Search
```csharp
.Bool(b => b
    .Must(m => m.MultiMatch(...))
    .Filter(f => f
        .Terms(t => t.Field("category")...)
        .Range(r => r.Field("price")...)
    )
)
.Aggregations(a => a
    .Terms("brands", ...)
    .Range("price_ranges", ...)
)
```

### 2. Log Analysis
- Time-based indexing
- Pattern detection
- Alerting

### 3. Document Search
- Full-text search
- Highlighting
- Suggestions

---

## SLIDE 23: Common Challenges & Solutions

| Challenge | Solution |
|-----------|----------|
| **Slow searches** | Use filters, optimize mappings |
| **Too many results** | Better relevance tuning |
| **Memory issues** | Pagination, limit fields |
| **Mapping conflicts** | Reindex with new mapping |
| **Split brain** | Proper cluster configuration |

### Debugging:
```csharp
.DisableDirectStreaming()  // See full request
.PrettyJson()              // Readable JSON
```

---

## SLIDE 24: Architecture Patterns

### 1. CQRS Pattern
- Commands → SQL Database
- Queries → Elasticsearch
- Sync via messaging

### 2. Event Sourcing
- Events → Elasticsearch
- Real-time analytics
- Audit trail

### 3. Microservices
- Centralized search
- Service discovery
- Distributed tracing

---

## SLIDE 25: Production Checklist

### Infrastructure:
- [ ] ✅ 3+ node cluster
- [ ] 💾 Proper disk space
- [ ] 🔄 Regular backups
- [ ] 📊 Monitoring (Kibana, Grafana)

### Security:
- [ ] 🔒 Enable X-Pack security
- [ ] 🔑 Role-based access
- [ ] 🛡️ SSL/TLS encryption
- [ ] 📝 Audit logging

### Optimization:
- [ ] ⚙️ Index lifecycle policies
- [ ] 🗄️ Hot-Warm-Cold architecture
- [ ] 🔧 Index templates
- [ ] 📈 Performance monitoring

---

## SLIDE 26: Learning Resources

### Official Documentation:
- 📚 [Elasticsearch Guide](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- 💻 [NEST Documentation](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html)
- 🎓 [Elastic Training](https://www.elastic.co/training/)

### Community:
- 💬 [Discuss Forum](https://discuss.elastic.co/)
- 🐙 [GitHub](https://github.com/elastic/elasticsearch-net)
- 📺 YouTube Tutorials

### Practice:
- 🏃 ეს demo project
- 🧪 Elasticsearch Playground
- 📝 Blog posts და articles

---

## SLIDE 27: Demo Summary

### რა ვნახეთ დღეს:

✅ **Elasticsearch API** - NEST client setup
✅ **Index Management** - Creation, templates, settings
✅ **Mappings** - 10+ data types
✅ **Indexing** - Single, bulk, update operations
✅ **Text Analysis** - Analyzers, tokenizers, filters
✅ **Query DSL** - 8+ query types
✅ **Advanced Search** - Aggregations, sorting, highlighting
✅ **Kibana** - Dev Tools და Discover
✅ **NEST** - .NET integration

---

## SLIDE 28: Key Takeaways

### 🎯 Main Points:

1. **Elasticsearch** = Search + Analytics Engine
2. **NEST** = Powerful .NET client
3. **Mappings** = Schema definition
4. **Analysis** = Text processing
5. **Query DSL** = Flexible search language
6. **Aggregations** = Analytics powerhouse
7. **Kibana** = Visualization და debugging

### Remember:
> "Elasticsearch is not a database, it's a search engine optimized for read-heavy workloads"

---

## SLIDE 29: Q&A

### კითხვები?

#### Possible Questions:
1. როდის გამოვიყენოთ Elasticsearch vs SQL?
2. როგორ მოვახდინოთ scale production-ზე?
3. როგორაა performance large datasets-თან?
4. რა არის განსხვავება text და keyword-ს შორის?
5. როგორ გამოვიყენოთ geo-search?

---

## SLIDE 30: Thank You!

### Contacts:
📧 Email: your.email@example.com
💼 LinkedIn: your-profile
🐙 GitHub: your-github

### Project Repository:
🔗 [GitHub Link]

### Next Steps:
1. 🚀 Try the demo project
2. 📚 Read official documentation
3. 🏗️ Build your own search
4. 💬 Join Elasticsearch community

**Thank you for your attention!** 🎉

---

## BONUS SLIDES

### Performance Metrics
- Indexing: ~10,000 docs/sec
- Search: <100ms for millions of docs
- Aggregations: Real-time analytics

### Version Compatibility
- Elasticsearch 7.x → NEST 7.x
- Elasticsearch 8.x → NEST 7.17+ or Elastic.Clients.Elasticsearch 8.x

### Monitoring
```csharp
GET _cluster/stats
GET _nodes/stats
GET _cat/health?v
```

