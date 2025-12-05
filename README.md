# Elasticsearch Level II - Demonstration Project

## 🎯 პროექტის მიზანი

ეს არის სრულფასოვანი Elasticsearch Level II დემონსტრაციული პროექტი, რომელიც მოიცავს ყველა მთავარ თემას პრეზენტაციისთვის.

## 📋 გაშუქებული თემები

### 1. **Elasticsearch API**
- NEST კლიენტის კონფიგურაცია
- ConnectionSettings და დაკავშირება
- Request/Response handling

### 2. **Creating Index and Templates**
- ინდექსების შექმნა custom settings-ებით
- Index Templates პატერნებისთვის
- Shards და Replicas კონფიგურაცია
- Custom Analyzers

### 3. **Data Types and Mappings**
- **Text** - სრული ტექსტური ძებნისთვის
- **Keyword** - ზუსტი დამთხვევებისთვის
- **Date** - თარიღები და დრო
- **Numeric** - რიცხვები (integer, long, double, float)
- **Boolean** - true/false მნიშვნელობები
- **Object** - JSON ობიექტები
- **Nested** - დამოუკიდებელი nested documents
- **Geo-point** - გეოგრაფიული კოორდინატები
- **IP** - IP მისამართები

### 4. **Indexing Documents**
- Single document indexing
- Bulk indexing (ეფექტური მასობრივი ჩაწერა)
- Update operations
- Delete operations
- Refresh strategies

### 5. **Text Analysis Basics**
- **Analyzers**: Standard, Simple, Whitespace, English, Custom
- **Tokenizers**: Standard, Keyword, Pattern, UAX URL Email
- **Token Filters**: Lowercase, Stop Words, Stemmer, Snowball
- Custom analyzer შექმნა

### 6. **Query DSL Basics**
- **Match Query** - სრული ტექსტური ძებნა
- **Multi-Match Query** - რამდენიმე ველში ძებნა
- **Term Query** - ზუსტი დამთხვევა
- **Terms Query** - მრავალი term-ის ძებნა
- **Range Query** - დიაპაზონული ძებნა
- **Bool Query** - queries-ების კომბინაცია (must, should, filter)
- **Wildcard Query** - wildcard პატერნები
- **Fuzzy Query** - typo tolerance

### 7. **Advanced Search**
- **Aggregations** - analytics და statistics
  - Terms aggregation (დაჯგუფება)
  - Stats aggregation (avg, min, max, sum)
  - Histogram aggregation
- **Sorting** - დახარისხება
- **Pagination** - from და size
- **Highlighting** - საძიებო ტერმინების მონიშვნა

### 8. **Kibana Basics**
- Dev Tools Console - queries-ების შესრულება
- Discover - ინტერაქტიული data exploration
- Index Patterns კონფიგურაცია
- KQL (Kibana Query Language)

### 9. **NEST and Elasticsearch.Net**
- NEST high-level client გამოყენება
- Strongly-typed queries
- Fluent API
- Async/await support

## 🚀 პროექტის გაშვება

### 🐳 გაშვება Docker-ით (რეკომენდებული)

**უმარტივესი გზა - ყველაფერი ერთი კლიკით:**

```cmd
build-and-start.bat
```

ეს script ავტომატურად:
1. ✅ ააგებს .NET აპლიკაციას ლოკალურად (Release)
2. ✅ შექმნის Docker images-ებს
3. ✅ გაუშვებს: Elasticsearch + Kibana + Demo App

**ან ნაბიჯ-ნაბიჯ:**

```cmd
# 1. .NET build ლოკალურად
cd ElasticSeach
dotnet build -c Release
cd ..

# 2. Docker build
docker-compose build

# 3. Start all services
docker-compose up -d
```

**შედეგი:**
- ✅ Elasticsearch: http://localhost:9200
- ✅ Kibana: http://localhost:5601

**ინტერაქტიული რეჟიმი (მენიუსთან სამუშაოდ):**

```cmd
docker-compose up -d elasticsearch kibana
docker-compose run --rm elasticsearch-demo
```

**დეტალური ინსტრუქციები**: [QUICKSTART.md](QUICKSTART.md) | [DOCKER_GUIDE.md](DOCKER_GUIDE.md)

---

### 💻 ლოკალური გაშვება (Docker-ის გარეშე)

#### წინაპირობები

1. **Elasticsearch** (Version 7.x ან 8.x)
   ```cmd
   # Docker-ით გაშვება
   docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" -e "xpack.security.enabled=false" docker.elastic.co/elasticsearch/elasticsearch:8.11.0
   ```

2. **Kibana** (Optional, but recommended)
   ```cmd
   # Docker-ით გაშვება
   docker run -d --name kibana --link elasticsearch:elasticsearch -p 5601:5601 docker.elastic.co/kibana/kibana:8.11.0
   ```

#### პროექტის გაშვება

```cmd
# Restore packages
dotnet restore

# Build project
dotnet build

# Run project
dotnet run
```

## 📁 პროექტის სტრუქტურა

```
ElasticSeach/
├── 🐳 Docker Files
│   ├── docker-compose.yml          # Docker Compose კონფიგურაცია
│   ├── Dockerfile                  # .NET აპლიკაციის Docker image
│   ├── .dockerignore              # Docker ignore rules
│   │
│   ├── check-system.bat           # სისტემის შემოწმება
│   ├── start-docker.bat           # სწრაფი გაშვება
│   ├── run-demo.bat               # ინტერაქტიული demo
│   ├── stop-docker.bat            # გაჩერება
│   │
│   ├── QUICKSTART.md              # 🚀 სწრაფი დაწყების გაიდი
│   └── DOCKER_GUIDE.md            # 📖 დეტალური Docker ინსტრუქციები
│
├── 📝 Documentation
│   ├── README.md                   # მთავარი დოკუმენტაცია
│   ├── PRESENTATION_SCRIPT.md
│   └── ...
│
└── ElasticSeach/                  # .NET Project
    ├── Models/
    │   ├── Product.cs              # პროდუქტის model
    │   └── Article.cs              # სტატიის model
    ├── Services/
    │   ├── IndexManagementService.cs
    │   ├── MappingService.cs
    │   ├── DocumentIndexingService.cs
    │   ├── TextAnalysisService.cs
    │   └── SearchService.cs
    ├── KibanaGuide/
    │   └── KibanaCommands.cs
    └── Program.cs                  # Main პროგრამა
```

## 🎮 პროგრამის გამოყენება

პროგრამას აქვს ინტერაქტიული მენიუ:

```
1. Index Management (Creating Index and Templates)
2. Data Types and Mappings
3. Indexing Documents
4. Text Analysis Basics
5. Query DSL Basics
6. Advanced Search Queries
7. Kibana Commands Guide
8. Run Complete Demonstration
0. Exit
```

## 📊 პრეზენტაციის სტრუქტურა

### Part 1: Setup and Configuration (5 წუთი)
- Elasticsearch-ის შესახებ მოკლე ინფო
- NEST client-ის კონფიგურაცია
- Connection testing

### Part 2: Index Management (10 წუთი)
- Index creation demo
- Settings configuration (shards, replicas)
- Index templates
- Custom analyzers setup

### Part 3: Mappings and Data Types (10 წუთი)
- სხვადასხვა data types-ის demo
- Explicit vs Dynamic mapping
- Field mappings showcase

### Part 4: Document Operations (10 წუთი)
- Single document indexing
- Bulk indexing performance
- Update და Delete operations
- Real data examples

### Part 5: Text Analysis (15 წუთი)
- Analyzers demonstration
- Tokenizers და Filters
- Custom analyzer creation
- Analysis API testing

### Part 6: Query DSL (20 წუთი)
- Basic queries showcase
- Bool queries complexity
- Real search scenarios
- Performance considerations

### Part 7: Advanced Features (15 წუთი)
- Aggregations examples
- Sorting და Pagination
- Highlighting
- Score calculation

### Part 8: Kibana Integration (15 წუთი)
- Dev Tools live demo
- Discover exploration
- Index patterns
- Visualization basics

## 🎯 Kibana Demo

### Dev Tools Console
1. გახსენი Kibana: http://localhost:5601
2. გადადი: Management > Dev Tools
3. გამოიყენე commands KibanaCommands.cs-დან

### Discover
1. შექმენი Index Pattern: `products*`
2. გადადი Analytics > Discover
3. გამოიყენე KQL queries:
   ```
   category: "Electronics"
   price >= 500 and price <= 1500
   name: *phone*
   tags: (laptop OR tablet)
   ```

## 📝 სასარგებლო ბრძანებები

### Elasticsearch Status
```bash
# Cluster health
curl http://localhost:9200/_cluster/health

# List indices
curl http://localhost:9200/_cat/indices?v
```

### Clean Up
```bash
# Delete all demo indices
curl -X DELETE http://localhost:9200/products
curl -X DELETE http://localhost:9200/articles
curl -X DELETE http://localhost:9200/data-types-demo
```

## 🎓 სასწავლო რესურსები

1. **Official Documentation**
   - [Elasticsearch Guide](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
   - [NEST Documentation](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html)

2. **Key Concepts**
   - Inverted Index
   - TF-IDF Scoring
   - Analysis Pipeline
   - Query Context vs Filter Context

3. **Best Practices**
   - Index naming conventions
   - Mapping optimization
   - Query performance
   - Bulk operations

## 💡 პრეზენტაციის Tips

1. **Live Demo**: უპირატესობა მიეცი live demo-ს კოდის ჩვენებაზე
2. **Real Examples**: გამოიყენე რეალური use cases-ები
3. **Kibana**: აჩვენე Kibana Dev Tools და Discover
4. **Performance**: ხაზგასმით აღნიშნე bulk operations-ის ეფექტურობა
5. **Q&A**: მოამზადე კითხვები scoring, performance, scaling თემებზე

## 🔍 Common Issues

### Connection Error
```
✗ Connection refused on localhost:9200
```
**გადაწყვეტა**: დარწმუნდი რომ Elasticsearch გაშვებულია

### Index Already Exists
```
✗ resource_already_exists_exception
```
**გადაწყვეტა**: პროგრამა ავტომატურად წაშლის და ხელახლა შექმნის indices

### Mapping Conflict
```
✗ mapper_parsing_exception
```
**გადაწყვეტა**: წაშალე index და ხელახლა შექმენი სწორი mapping-ით

## 📞 დახმარება

თუ რაიმე კითხვა გაქვს:
- Elasticsearch Forum: https://discuss.elastic.co/
- NEST GitHub: https://github.com/elastic/elasticsearch-net
- Stack Overflow: tag `elasticsearch` და `nest`

## ✅ Checklist პრეზენტაციამდე

### Docker Setup (რეკომენდებული)
- [ ] Docker Desktop დაინსტალირებული და გაშვებული
- [ ] `check-system.bat` - ყველა შემოწმება გავლილი
- [ ] `start-docker.bat` - სერვისები გაშვებული
- [ ] Elasticsearch რეაგირებს: http://localhost:9200
- [ ] Kibana ხელმისაწვდომია: http://localhost:5601
- [ ] `run-demo.bat` - აპლიკაცია მუშაობს ინტერაქტიულად
- [ ] Kibana Dev Tools ნატესტია
- [ ] Discover-ში index patterns (`products*`, `articles*`) კონფიგურირებულია

### ან ლოკალური Setup
- [ ] Elasticsearch გაშვებული და მუშაობს
- [ ] Kibana ხელმისაწვდომია
- [ ] პროექტი კომპილირდება და გაშვებულია
- [ ] Index-ები შექმნილია და data ჩაწერილია
- [ ] Kibana Dev Tools ნატესტია
- [ ] Discover-ში index patterns კონფიგურირებულია

### Demo მზადყოფნა
- [ ] Sample queries მზადაა
- [ ] კითხვების პასუხები მოფიქრებული
- [ ] ლოგები სუფთაა (თუ საჭიროა: `docker-compose restart`)

### Quick Commands
```cmd
# გაშვება
start-docker.bat

# ინტერაქტიული demo
run-demo.bat

# ლოგების ნახვა
docker-compose logs -f

# გაჩერება
stop-docker.bat

# რესტარტი (სუფთა ლოგებისთვის)
docker-compose restart
```

---

**წარმატებები პრეზენტაციაში! 🎉**

