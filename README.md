# ElasticSearch Demo Project

.NET 9.0 application for synchronizing data from SQL Server to Elasticsearch using Hangfire background jobs.

## 🏗️ Architecture

- **ElasticSearch.Api** - ASP.NET Core Web API
- **ElasticSearch.Jobs** - Background Jobs Service (Hangfire)
- **ElasticSearch.Core** - Shared business logic and services
- **SQL Server** - Primary data store (LocalDB or SQL Server container)
- **Elasticsearch** - Search engine
- **Kibana** - Elasticsearch UI

---

## 🚀 Getting Started

### ✅ Option 1: Local Development (Windows + LocalDB)

**Recommended for local development**

```bash
# 1. Elasticsearch and Kibana (in Docker)
docker-compose up -d elasticsearch kibana

# 2. Jobs Service (local)
cd ElasticSearch.Jobs
dotnet run

# 3. API (local, in a new terminal)
cd ElasticSearch.Api
dotnet run
```

**Connection String**: `Server=(localdb)\MSSQLLocalDB` - Integrated Security ✅

---

### 🐳 Option 2: Full Docker Stack

**Elasticsearch, API, Jobs - everything in Docker**

#### First run:

```powershell
# 1. Publish .NET projects
dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release
dotnet publish ElasticSearch.Jobs/ElasticSearch.Jobs.csproj -c Release

# 2. Build Docker images
docker-compose build --no-cache

# 3. Start all services
docker-compose up -d
```

#### After code changes:

```powershell
# 1. Stop containers
docker-compose down

# 1.1 if removing volumes is needed
docker volume rm elasticsearch_elasticsearch-data elasticsearch_sqlserver-data

# 2. Republish changed projects
dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release
dotnet publish ElasticSearch.Jobs/ElasticSearch.Jobs.csproj -c Release

# 3. Rebuild and restart
docker-compose build --no-cache api jobs
docker-compose up -d
```

**Connection String**: `Server=sqlserver,1433` - SQL Authentication (sa/Password1234!) ✅

---

## 📊 Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| **API** | http://localhost:5000 | - |
| **Elasticsearch** | http://localhost:9200 | - |
| **Kibana** | http://localhost:5601 | - |
| **SQL Server (Docker)** | localhost:1433 | sa / Password1234! |
| **SQL Server (LocalDB)** | (localdb)\MSSQLLocalDB | Integrated Security |

---

## 🔄 Background Sync Jobs

Hangfire automatically performs synchronization:

- **Incremental Sync**: Every 5 minutes (changes only)
- **Full Sync (Daily)**: Every day at 02:00 AM
- **Full Sync (Weekly)**: Every Sunday at 03:00 AM

---

## 📁 Project Structure

```
ElasticSearch/
├── ElasticSearch.Api/              # Web API
│   ├── Controllers/
│   ├── Program.cs
│   ├── Dockerfile
│   ├── appsettings.json           # LocalDB config
│   └── appsettings.Production.json # Docker config
│
├── ElasticSearch.Jobs/             # Background Jobs
│   ├── Program.cs
│   ├── Dockerfile
│   ├── appsettings.json           # LocalDB config
│   └── appsettings.Production.json # Docker config
│
├── ElasticSearch.Core/             # Shared Library
│   ├── Models/
│   │   └── Product.cs
│   └── Services/
│       ├── DatabaseSeederService.cs
│       ├── IndexMappingService.cs
│       ├── ProductSearchService.cs
│       ├── AdvancedSearchService.cs
│       └── SqlToElasticsearchSyncService.cs
│
├── docker-compose.yml              # Infrastructure
└── README.md                       # Documentation
```

---

## 🔍 Search Features Implemented

The project demonstrates various Elasticsearch search capabilities:

### 1. Simple Search (`ProductSearchService`)
- Full-text search across multiple fields (Name, Description, Category).
- Filtering by Category and Price Range.
- Terms Aggregation for category statistics.

### 2. Advanced Search (`AdvancedSearchService`)
- **Complex Bool Query**: Combining `must`, `filter`, and `should` for precise results.
- **Fuzzy Search**: Tolerance for typos (e.g., searching for "mackbook" finds "macbook").
- **Prefix Search / Autocomplete**: Optimized for search-as-you-type functionality.
- **Pagination**: Efficiently handling large result sets using `from` and `size`.
- **Highlighting**: Visually marking matching terms in search results.

### 3. Index Management (`IndexMappingService`)
- **Custom Analyzers**: Stemming, lowercase, and synonym filters (e.g., laptop = notebook).
- **Edge N-Gram Tokenizer**: Powering the autocomplete feature.
- **Reindexing**: Moving data between indices with zero downtime.
- **Analysis API**: Testing how text is tokenized by different analyzers.

---

## 💡 Key Elasticsearch Concepts Demonstrated

- **Inverted Index**: The core data structure for fast full-text searching.
- **Analysis Pipeline**: Tokenizers and Filters (Stop words, Stemming, Synonyms).
- **Shards & Replicas**: Horizontal scaling and high availability.
- **Scoring (BM25)**: How relevance is calculated using TF-IDF.
- **Mapping**: Defining field types (Text vs Keyword) for optimal performance.
- **Refresh Interval**: Balancing write performance and search near-real-time visibility.

---

## 🛠️ Technologies Used

- **.NET 9.0**
- **Elasticsearch 8.11**
- **Kibana 8.11**
- **NEST** (.NET Client for Elasticsearch)
- **Hangfire** (Background Jobs)
- **Dapper** (Lightweight ORM for SQL)
- **SQL Server 2022**
- **Docker & Docker Compose**
