# 🚀 Elasticsearch Solution - 3 პროექტი

## 📁 სტრუქტურა

```
ElasticSearch.sln
│
├── ElasticSearch.Core/          # Shared Library
│   ├── Models/
│   │   └── Product.cs
│   └── Services/
│       ├── SqlToElasticsearchSyncService.cs
│       └── ProductSearchService.cs
│
├── ElasticSearch.Api/           # Web API + Swagger
│   ├── Controllers/
│   │   └── ProductsController.cs
│   ├── Program.cs
│   └── appsettings.json
│
└── ElasticSearch.Jobs/          # Background Jobs (Hangfire)
    ├── Program.cs
    └── appsettings.json
```

---

## ⚙️ Setup

### 1️⃣ SQL Server Setup

```cmd
sqlcmd -S localhost -U sa -P YourPassword -i SqlServer_Setup.sql
```

ან SSMS-ში გაუშვით: `SqlServer_Setup.sql`

### 2️⃣ Configure Connection Strings

**ElasticSearch.Api/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=ElasticsearchDemo;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}
```

**ElasticSearch.Jobs/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=ElasticsearchDemo;Integrated Security=True;TrustServerCertificate=True;",
    "Hangfire": "Server=localhost;Database=ElasticsearchDemo;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}
```

### 3️⃣ Restore & Build

```cmd
dotnet restore
dotnet build
```

---

## 🎮 როგორ გავუშვათ

### Variant 1: ცალ-ცალკე Terminal-ებში

**Terminal 1 - API:**
```cmd
cd ElasticSearch.Api
dotnet run
```
API გაეშვება: http://localhost:5000
Swagger: http://localhost:5000

**Terminal 2 - Background Jobs:**
```cmd
cd ElasticSearch.Jobs
dotnet run
```

### Variant 2: Visual Studio / Rider-ით

1. Right-click solution → Set Startup Projects
2. Select: Multiple startup projects
3. Set both `ElasticSearch.Api` and `ElasticSearch.Jobs` to **Start**
4. Press F5

---

## 📡 API Endpoints (Swagger)

გახსენი: **http://localhost:5000**

### Available Endpoints:

```
GET /api/products/search?query=laptop
    └─ ძებნა query-ით

GET /api/products/{id}
    └─ პროდუქტის მიღება ID-ით

GET /api/products/category/{category}
    └─ კატეგორიის მიხედვით

GET /api/products/price-range?min=100&max=1000
    └─ ფასის დიაპაზონით

GET /api/products/categories/stats
    └─ სტატისტიკა კატეგორიების მიხედვით
```

### მაგალითები:

```bash
# Search
curl http://localhost:5000/api/products/search?query=laptop

# Get by ID
curl http://localhost:5000/api/products/1

# By category
curl http://localhost:5000/api/products/category/Electronics

# Price range
curl http://localhost:5000/api/products/price-range?min=500&max=2000

# Stats
curl http://localhost:5000/api/products/categories/stats
```

---

## 🔄 Background Jobs

Background Jobs სერვისი ავტომატურად:

### 1. Incremental Sync - ყოველ 5 წუთში
```
SQL Server → Elasticsearch
(მხოლოდ შეცვლილი records)
```

### 2. Full Sync - ყოველ დღე 2:00 საათზე
```
SQL Server → Elasticsearch
(ყველა active product)
```

### 3. Full Sync - ყოველ კვირას 3:00 საათზე
```
Weekly maintenance sync
```

### ლოგების ნახვა:

```cmd
cd ElasticSearch.Jobs
dotnet run
```

Output:
```
╔══════════════════════════════════════════════════════════════╗
║     ELASTICSEARCH BACKGROUND JOBS SERVICE                    ║
╚══════════════════════════════════════════════════════════════╝

🔗 SQL Server: Server=localhost
🔍 Elasticsearch: http://localhost:9200

✅ Connected to Elasticsearch
✅ Hangfire configured

📅 Setting up recurring jobs:
  ✓ Incremental Sync: Every 5 minutes
  ✓ Full Sync: Daily at 2:00 AM
  ✓ Full Sync: Weekly (Sunday 3:00 AM)

╔══════════════════════════════════════════════════════════════╗
║     🚀 BACKGROUND JOBS SERVER STARTED                       ║
╚══════════════════════════════════════════════════════════════╝

🔥 Triggering initial full sync...
🔄 [FULL SYNC] Starting...
📊 Found 10 products
✅ [FULL SYNC] Completed
```

---

## 🧪 Testing Flow

### 1. გაუშვით SQL Server და Elasticsearch

```cmd
# Elasticsearch (Docker):
docker run -d -p 9200:9200 -e "discovery.type=single-node" -e "xpack.security.enabled=false" docker.elastic.co/elasticsearch/elasticsearch:8.11.0

# SQL Server უკვე დაინსტალირებული უნდა იყოს
```

### 2. Setup Database

```cmd
sqlcmd -S localhost -i SqlServer_Setup.sql
```

### 3. გაუშვით Background Jobs

```cmd
cd ElasticSearch.Jobs
dotnet run
```

დაელოდეთ: "✅ [FULL SYNC] Completed"

### 4. გაუშვით API

```cmd
cd ElasticSearch.Api
dotnet run
```

### 5. გატესტეთ Swagger-ით

გახსენით: http://localhost:5000

Try it out:
- GET /api/products/search?query=laptop
- Expected: პროდუქტების სია

---

## 📊 არქიტექტურა

```
┌─────────────────┐
│  SQL Server     │
│  (Master Data)  │
└────────┬────────┘
         │
         │ Hangfire Background Jobs
         │ (Every 5 minutes)
         ▼
┌─────────────────┐
│ Elasticsearch   │
│ (Search Engine) │
└────────┬────────┘
         │
         │ REST API
         ▼
┌─────────────────┐
│   API Client    │
│ (Swagger/Apps)  │
└─────────────────┘
```

### Data Flow:

1. **SQL Server** - Master database (CRUD operations)
2. **Background Jobs** - Sync SQL → Elasticsearch
3. **Elasticsearch** - Fast search index
4. **API** - Expose search endpoints
5. **Clients** - Consume API (Swagger, Frontend, etc.)

---

## 🎯 Use Cases

### API პროექტი:
- ✅ Product search (full-text)
- ✅ Category filtering
- ✅ Price range queries
- ✅ Statistics/Aggregations
- ✅ REST API for frontend

### Background Jobs პროექტი:
- ✅ Auto-sync SQL → Elasticsearch
- ✅ Incremental updates (every 5 min)
- ✅ Full sync (daily/weekly)
- ✅ Runs independently
- ✅ No API overhead

---

## 📈 Performance

### 100,000 Products:

**Full Sync:**
- Time: ~2-3 minutes
- Frequency: Daily at 2 AM

**Incremental Sync:**
- Time: ~5-10 seconds (for 100 changes)
- Frequency: Every 5 minutes

**API Search:**
- Response: 10-50ms
- Concurrent requests: 100+

---

## 🔧 Configuration

### Cron Expressions:

```csharp
"*/5 * * * *"    // Every 5 minutes
"0 2 * * *"      // Daily at 2:00 AM
"0 3 * * 0"      // Sunday at 3:00 AM
```

### Customize in appsettings.json:

```json
"BackgroundJobs": {
  "IncrementalSyncCron": "*/10 * * * *",  // Every 10 min
  "FullSyncDailyCron": "0 3 * * *",       // 3 AM
  "FullSyncWeeklyCron": "0 4 * * 0"       // Sunday 4 AM
}
```

---

## 🐛 Troubleshooting

### API არ იწყება:

```cmd
# Check port:
netstat -ano | findstr :5000

# Change port in launchSettings.json if needed
```

### Background Jobs error:

```cmd
# Check SQL Server connection:
sqlcmd -S localhost -Q "SELECT @@VERSION"

# Check Hangfire tables:
SELECT * FROM HangfireJobs.Job
```

### Elasticsearch connection failed:

```cmd
# Check Elasticsearch:
curl http://localhost:9200

# If not running:
docker run -d -p 9200:9200 -e "discovery.type=single-node" -e "xpack.security.enabled=false" docker.elastic.co/elasticsearch/elasticsearch:8.11.0
```

---

## ✅ Checklist

- [ ] SQL Server დაინსტალირდა და გაშვებულია
- [ ] `SqlServer_Setup.sql` გაეშვა
- [ ] Elasticsearch გაშვებულია (port 9200)
- [ ] Connection strings კონფიგურირებულია
- [ ] `dotnet restore && dotnet build` წარმატებით დასრულდა
- [ ] Background Jobs გაშვებულია და sync-ავს
- [ ] API გაშვებულია (http://localhost:5000)
- [ ] Swagger იხსნება და endpoints მუშაობს

---

## 🎉 Summary

**3 დამოუკიდებელი პროექტი:**

1. **ElasticSearch.Core** - Shared library (models + services)
2. **ElasticSearch.Api** - REST API + Swagger (port 5000)
3. **ElasticSearch.Jobs** - Background jobs (Hangfire)

**ორივე ერთდროულად უნდა გაშვებული იყოს production-ში!**

**წარმატებები! 🚀**

