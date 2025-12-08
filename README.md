# ElasticSearch Demo Project

.NET 9.0 აპლიკაცია SQL Server-დან Elasticsearch-ში მონაცემების სინქრონიზაციისთვის Hangfire background jobs-ით.

## 🏗️ არქიტექტურა

- **ElasticSearch.Api** - ASP.NET Core Web API
- **ElasticSearch.Jobs** - Background Jobs Service (Hangfire)
- **ElasticSearch.Core** - Shared business logic და services
- **SQL Server** - Primary data store (LocalDB ან SQL Server container)
- **Elasticsearch** - Search engine
- **Kibana** - Elasticsearch UI

---

## 🚀 გაშვება

### ✅ ვარიანტი 1: Local Development (Windows + LocalDB)

**რეკომენდებული local development-სთვის**

```bash
# 1. Elasticsearch და Kibana (Docker-ში)
docker-compose up -d elasticsearch kibana

# 2. Jobs Service (local)
cd ElasticSearch.Jobs
dotnet run

# 3. API (local, ახალ terminal-ში)
cd ElasticSearch.Api
dotnet run
```

**Connection String**: `Server=(localdb)\MSSQLLocalDB` - Integrated Security ✅

---

### 🐳 ვარიანტი 2: Full Docker Stack

**SQL Server, Elasticsearch, API, Jobs - ყველაფერი Docker-ში**

```bash
# ყველაფრის გაშვება
docker-compose up -d

# ან rebuild-ით
docker-compose up -d --build
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

Hangfire ავტომატურად ასრულებს სინქრონიზაციას:

- **Incremental Sync**: ყოველ 5 წუთში (მხოლოდ ცვლილებები)
- **Full Sync (Daily)**: ყოველდღე 02:00 AM
- **Full Sync (Weekly)**: ყოველ კვირას 03:00 AM

---

## 📁 პროექტის სტრუქტურა

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
│       ├── SqlToElasticsearchSyncService.cs
│       └── SyncJobExecutor.cs
│
├── docker-compose.yml              # Docker orchestration
├── .dockerignore
└── .gitignore
```

---

## 🔧 Configuration

### Local Development (appsettings.json)
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=ElasticsearchDemo;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}
```

### Docker (appsettings.Production.json)
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=sqlserver,1433;Database=ElasticsearchDemo;User Id=sa;Password=Password1234!;TrustServerCertificate=True;"
  },
  "Elasticsearch": {
    "Uri": "http://elasticsearch:9200"
  }
}
```

---

## ✨ Features

### ✅ Automatic Database Initialization
- თავისთავად ქმნის database და tables
- ატვირთავს 15 სატესტო პროდუქტს (ქართულ-ინგლისური descriptions)
- ქმნის indexes და constraints

### ✅ API Endpoints

#### Search Products
```http
GET /api/products/search?query=laptop&pageSize=10&pageNumber=1
```

#### Advanced Search
```http
POST /api/advanced-search
Content-Type: application/json

{
  "query": "macbook",
  "category": "Laptops",
  "minPrice": 1000,
  "maxPrice": 5000,
  "tags": ["apple", "premium"],
  "pageSize": 20,
  "pageNumber": 1
}
```

#### Index Management
```http
GET /api/index/health
GET /api/index/mapping
POST /api/index/recreate
```

### ✅ Background Synchronization
- **Incremental Sync**: აღმოაჩენს მხოლოდ ცვლილებებს (UpdatedAt field-ის მიხედვით)
- **Full Sync**: სრული რესინქრონიზაცია
- **Automatic timestamp tracking**: ყოველი sync-ის შემდეგ ინახავს timestamp-ს Elasticsearch-ში

---

## 🛠️ Development

### Prerequisites
- .NET 9.0 SDK
- Docker Desktop
- SQL Server LocalDB (Windows) ან SQL Server 2022

### Build
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests (თუ არის)
dotnet test
```

### Docker Build
```bash
# Build all services
docker-compose build

# Build specific service
docker-compose build api
docker-compose build jobs

# No cache build
docker-compose build --no-cache
```

---

## 🐛 Troubleshooting

### LocalDB არ მუშაობს Docker-ში
LocalDB არის Windows-only და Docker Linux containers-ში არ მუშაობს.  
**გადაწყვეტა**: გამოიყენეთ Hybrid setup (Elasticsearch Docker-ში, API/Jobs locally).

### SQL Server container არ ეშვება
```bash
# Check logs
docker-compose logs sqlserver

# Restart
docker-compose restart sqlserver

# Clean restart
docker-compose down -v
docker-compose up -d
```

### Elasticsearch არ არის healthy
```bash
# Check cluster health
curl http://localhost:9200/_cluster/health

# Check logs
docker-compose logs elasticsearch
```

---

## 📝 Notes

- **Production**: არ გამოიყენოთ default პაროლები production-ში
- **Security**: `.env` ფაილი დამატებულია `.gitignore`-ში
- **LocalDB**: იდეალურია local development-სთვის, არა production-სთვის
- **Docker**: სრული stack Docker-ში - production-ready setup

---

## 📄 License

MIT License

