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

**Elasticsearch, API, Jobs - ყველაფერი Docker-ში**

#### პირველი გაშვება:

```powershell
# 1. Publish .NET პროექტები
dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release
dotnet publish ElasticSearch.Jobs/ElasticSearch.Jobs.csproj -c Release

# 2. Build Docker images
docker-compose build --no-cache

# 3. Start all services
docker-compose up -d
```

#### კოდის ცვლილების შემდეგ:

```powershell
# 1. Stop containers
docker-compose down

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

## 🧪 ტესტირება

### Endpoints-ის ტესტირება

```powershell
# ყველა endpoint-ის ტესტირება
.\test-endpoints.ps1

# Autocomplete endpoint-ის ტესტირება
.\test-autocomplete.ps1
```

### ხელით ტესტირება (curl)

```powershell
# Basic Search
curl.exe "http://localhost:5000/api/products/search?query=macbook"

# Autocomplete
curl.exe "http://localhost:5000/api/advancedsearch/autocomplete?prefix=mac"

# Category Filter
curl.exe "http://localhost:5000/api/products/category/laptops"

# Fuzzy Search
curl.exe "http://localhost:5000/api/advancedsearch/fuzzy?query=mackbok"

# Complex Search with Filters
curl.exe "http://localhost:5000/api/advancedsearch/complex?query=macbook&category=laptops&maxPrice=3000"
```

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

**მნიშვნელოვანი**: Docker არ აბილდებს კოდს - იყენებს pre-published files-ს

```powershell
# 1. Publish ცვლილებები
dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release
dotnet publish ElasticSearch.Jobs/ElasticSearch.Jobs.csproj -c Release

# 2. Build Docker images
docker-compose build --no-cache

# 3. Start containers
docker-compose up -d
```

**რატომ ასე?**
- ✅ ამცირებს Docker build time-ს
- ✅ თავიდან აიცილებს SSL certificate პრობლემებს NuGet restore-ში
- ✅ უზრუნველყოფს რომ ყველაზე ბოლო ცვლილებები Docker-ში იქნება

---

## 🐛 Troubleshooting

### API არ აბრუნებს შედეგებს
```powershell
# შეამოწმე რომ publish გაკეთდა
dir ElasticSearch.Api\bin\Release\net9.0\publish\ElasticSearch.Api.dll

# თუ არ არსებობს, გააკეთე publish
dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release

# Rebuild Docker image
docker-compose build --no-cache api
docker-compose up -d
```

### კოდის ცვლილებები არ ჩანს Docker-ში
```powershell
# 1. Stop all containers
docker-compose down

# 2. Republish
dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release
dotnet publish ElasticSearch.Jobs/ElasticSearch.Jobs.csproj -c Release

# 3. Rebuild და Restart
docker-compose build --no-cache api jobs
docker-compose up -d
```

### Elasticsearch არ არის healthy
```powershell
# Check cluster health
curl.exe http://localhost:9200/_cluster/health

# Check logs
docker-compose logs elasticsearch

# Restart
docker-compose restart elasticsearch
```

### Containers-ის სტატუსის შემოწმება
```powershell
# ყველა container
docker ps

# Specific container logs
docker-compose logs api
docker-compose logs jobs
docker-compose logs elasticsearch
```

---

## 📝 Notes

- ✅ **Docker არ აბილდებს კოდს** - იყენებს pre-published files-ს local build-იდან
- ✅ **კოდის ცვლილებისას** - ყოველთვის გააკეთე `dotnet publish` და შემდეგ `docker-compose build`
- ✅ **bin/ და obj/ folders** - `.gitignore`-ში დაემატა, მაგრამ `bin/Release/net9.0/publish` არ არის ignore-დ
- ✅ **SSL პრობლემების თავიდან ასაცილებლად** - local publish გამოიყენება
- ✅ **ტესტის სკრიპტები** - `test-endpoints.ps1` და `test-autocomplete.ps1` ყველა endpoint-ის შესამოწმებლად

---

## 📄 License

MIT License

