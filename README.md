# ElasticSearch Demo Project

Full-stack .NET 9.0 application demonstrating SQL Server to Elasticsearch synchronization with Hangfire background jobs.

## 🏗️ Architecture

- **ElasticSearch.Api** - ASP.NET Core Web API
- **ElasticSearch.Jobs** - Background Jobs Service (Hangfire)
- **ElasticSearch.Core** - Shared business logic and services
- **SQL Server** - Primary data store
- **Elasticsearch** - Search engine
- **Kibana** - Elasticsearch UI

## 🚀 Quick Start

### 🏠 Option 1: Local Development (Windows + LocalDB) - RECOMMENDED FOR DEVELOPMENT

**Best for**: Windows development with Integrated Security (no passwords needed)

See detailed guide: [LOCAL-SETUP.md](LOCAL-SETUP.md)

```bash
# 1. Start Elasticsearch & Kibana only
start-local.bat

# 2. Run Jobs
cd ElasticSearch.Jobs
dotnet run

# 3. Run API (new terminal)
cd ElasticSearch.Api
dotnet run
```

**Connection String**: `Server=(localdb)\MSSQLLocalDB` with Integrated Security ✅

---

### 🐳 Option 2: Full Docker Stack - FOR PRODUCTION OR TEAM COLLABORATION

**Best for**: Production deployment, Linux/Mac, team environments

See detailed guide: [QUICKSTART.md](QUICKSTART.md)

```bash
# Setup .env file (optional)
copy .env.example .env

# Start everything
docker-start.bat
# OR
docker-compose up -d
```

**Connection String**: `Server=sqlserver,1433` with SQL Authentication (sa/password)

---

## 📊 Setup Comparison

| Feature | 🏠 Local (LocalDB) | 🐳 Docker (Full Stack) |
|---------|----------------|-------------------|
| **OS Support** | Windows only | Windows/Linux/Mac |
| **SQL Auth** | Integrated Security | SQL Auth (sa/password) |
| **Setup Time** | ⚡ Fast (seconds) | 🐢 Slower (minutes) |
| **Resources** | 💾 Light (~500MB) | 💾 Heavy (8GB+ RAM) |
| **Debugging** | ✅ Easy (native) | ⚠️ Medium (containers) |
| **Production** | ❌ Development only | ✅ Production ready |
| **Team Sharing** | ⚠️ Windows required | ✅ Cross-platform |
| **Database** | LocalDB | SQL Server 2022 |

**💡 Recommendation**:  
- Use **Local** for solo Windows development
- Use **Docker** for production, Linux/Mac, or team projects

---

#### Prerequisites
- Docker Desktop installed
- 8GB+ RAM available for Docker

#### Start All Services

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes (clean restart)
docker-compose down -v
```

#### Access Services

- **API**: http://localhost:5000
- **Elasticsearch**: http://localhost:9200
- **Kibana**: http://localhost:5601
- **SQL Server**: localhost,1433 (sa / YourStrong@Password123)

#### Run Individual Services

```bash
# Run only infrastructure (SQL + Elasticsearch + Kibana)
docker-compose up -d sqlserver elasticsearch kibana

# Run API only
docker-compose up -d api

# Run Jobs only
docker-compose up -d jobs
```

### Option 2: Run Locally with .NET

#### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Elasticsearch running on localhost:9200

#### Setup

1. **Start Elasticsearch and Kibana** (via Docker):
```bash
docker-compose up -d elasticsearch kibana
```

2. **Update connection strings** in `appsettings.json` files:
   - ElasticSearch.Api/appsettings.json
   - ElasticSearch.Jobs/appsettings.json

3. **Run API**:
```bash
cd ElasticSearch.Api
dotnet run
```

4. **Run Jobs Service** (in separate terminal):
```bash
cd ElasticSearch.Jobs
dotnet run
```

## 📊 Features

### Automatic Database Initialization
- Creates database if not exists
- Creates Products table with indexes
- Seeds 15 test products (Georgian + English descriptions)

### Background Synchronization
- **Incremental Sync**: Every 5 minutes (detects changes)
- **Full Sync**: Daily at 2:00 AM
- **Weekly Sync**: Sunday at 3:00 AM

### API Endpoints

#### Product Search
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

## 🔧 Configuration

### appsettings.json (API)

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=ElasticsearchDemo;Trusted_Connection=True;"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}
```

### appsettings.json (Jobs)

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=ElasticsearchDemo;Trusted_Connection=True;",
    "Hangfire": "Server=(localdb)\\MSSQLLocalDB;Database=ElasticsearchDemo;Trusted_Connection=True;"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  },
  "BackgroundJobs": {
    "IncrementalSyncCron": "*/5 * * * *",
    "FullSyncDailyCron": "0 2 * * *",
    "FullSyncWeeklyCron": "0 3 * * 0"
  }
}
```

## 🧪 Testing

### Test Data Operations

**Add new product** (SQL Server):
```sql
INSERT INTO Products (Name, Description, Price, Stock, Category, Tags, Brand, Model, CreatedDate, UpdatedAt, IsActive, IsDeleted)
VALUES ('Test Product', 'სატესტო პროდუქტი', 99.99, 10, 'Test', 'test', 'TestBrand', 'Model1', GETDATE(), GETDATE(), 1, 0);
```

**Update product** (will sync in 5 minutes):
```sql
UPDATE Products 
SET Price = 5999.99, UpdatedAt = GETDATE() 
WHERE Id = 1;
```

**Soft delete** (will remove from Elasticsearch):
```sql
UPDATE Products 
SET IsDeleted = 1, UpdatedAt = GETDATE() 
WHERE Id = 15;
```

### Search in Kibana

1. Open Kibana: http://localhost:5601
2. Go to Dev Tools
3. Run queries:

```json
GET /products/_search
{
  "query": {
    "match": {
      "name": "laptop"
    }
  }
}
```

## 📦 Docker Services

| Service | Container | Port | Description |
|---------|-----------|------|-------------|
| SQL Server | sqlserver | 1433 | Primary database |
| Elasticsearch | elasticsearch | 9200, 9300 | Search engine |
| Kibana | kibana | 5601 | Elasticsearch UI |
| API | elasticsearch-api | 5000 (→8080) | Web API |
| Jobs | elasticsearch-jobs | - | Background sync |

## 🛠️ Development

### Build Docker Images

```bash
# Build all services
docker-compose build

# Build specific service
docker-compose build api
docker-compose build jobs

# Rebuild without cache
docker-compose build --no-cache
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f jobs

# Last 100 lines
docker-compose logs --tail=100 jobs
```

### Database Access

```bash
# Connect to SQL Server container
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123

# Run query
1> SELECT COUNT(*) FROM ElasticsearchDemo.dbo.Products;
2> GO
```

## 🐛 Troubleshooting

### Jobs not syncing?
- Check SQL Server connection
- Verify Elasticsearch is running: `curl http://localhost:9200`
- Check Hangfire dashboard in logs
- Ensure database has data

### Docker containers failing?
```bash
# Check container status
docker-compose ps

# View specific container logs
docker-compose logs api

# Restart service
docker-compose restart api

# Clean restart
docker-compose down -v
docker-compose up -d
```

### Connection issues?
- Ensure all containers are on same network
- Check health checks: `docker-compose ps`
- Verify connection strings use container names (not localhost)

## 📝 Project Structure

```
ElasticSearch/
├── ElasticSearch.Api/           # Web API
│   ├── Controllers/
│   ├── Program.cs
│   ├── Dockerfile
│   └── appsettings.json
├── ElasticSearch.Jobs/          # Background Jobs
│   ├── Program.cs
│   ├── Dockerfile
│   └── appsettings.json
├── ElasticSearch.Core/          # Shared Library
│   ├── Models/
│   └── Services/
├── docker-compose.yml           # Docker Compose config
├── .dockerignore               # Docker ignore rules
└── .gitignore                  # Git ignore rules
```

## 🔐 Security Notes

⚠️ **Default passwords are for development only!**

For production:
- Change SQL Server SA password
- Enable Elasticsearch security
- Use environment variables
- Enable HTTPS
- Implement authentication

## 📄 License

MIT License - feel free to use in your projects!

