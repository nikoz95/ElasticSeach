│       ├── SqlToElasticsearchSyncService.cs
│       └── ProductSearchService.cs
│
├── ElasticSearch.Api/           ✅ Web API + Swagger
│   ├── Controllers/ProductsController.cs
│   ├── Program.cs
│   └── appsettings.json
│
└── ElasticSearch.Jobs/          ✅ Background Jobs
    ├── Program.cs
    └── appsettings.json
```

---

## 🚀 როგორ გავუშვათ:

### Option 1: ორივე ერთდროულად (რეკომენდებული)

```cmd
start-all.bat
```

გაიხსნება 2 terminal ცალ-ცალკე:
- ✅ API: http://localhost:5000
- ✅ Background Jobs

### Option 2: ცალ-ცალკე

**Terminal 1 - API:**
```cmd
start-api.bat
```

**Terminal 2 - Background Jobs:**
```cmd
start-jobs.bat
```

---

## 📡 API Testing (Swagger)

გახსენი: **http://localhost:5000**

### Endpoints:

1. **GET /api/products/search?query=laptop**
   - ძებნა სიტყვით

2. **GET /api/products/{id}**
   - პროდუქტი ID-ით

3. **GET /api/products/category/{category}**
   - კატეგორიით

4. **GET /api/products/price-range?min=100&max=1000**
   - ფასის დიაპაზონით

5. **GET /api/products/categories/stats**
   - სტატისტიკა

### cURL მაგალითები:

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

Background Jobs ავტომატურად სინქრონიზაციას უკეთებს:

✅ **Incremental Sync** - ყოველ 5 წუთში  
✅ **Full Sync (Daily)** - ყოველ დღე 2:00 AM  
✅ **Full Sync (Weekly)** - კვირას 3:00 AM  

---

## 📋 Checklist:

- [x] ✅ SQL Server Setup (`SqlServer_Setup.sql`)
- [x] ✅ Connection strings configured
- [x] ✅ Elasticsearch running (port 9200)
- [x] ✅ Solution build წარმატებით
- [x] ✅ API starts on http://localhost:5000
- [x] ✅ Background Jobs running
- [x] ✅ Swagger UI იხსნება

---

## 🎯 რა მიიღეთ:

### 3 დამოუკიდებელი პროექტი:

1. **ElasticSearch.Core** - Shared library
2. **ElasticSearch.Api** - REST API + Swagger
3. **ElasticSearch.Jobs** - Background sync service

### ორივე ერთდროულად:

```
┌─────────────────┐
│  SQL Server     │  (Master Data)
└────────┬────────┘
         │
         │ Background Jobs (Every 5 min)
         ▼
┌─────────────────┐
│ Elasticsearch   │  (Search Engine)
└────────┬────────┘
         │
         │ REST API
         ▼
┌─────────────────┐
│  Swagger UI     │  http://localhost:5000
└─────────────────┘
```

---

## 📚 დოკუმენტაცია:

- **NEW_ARCHITECTURE_README.md** - სრული გაიდი
- **SqlServer_Setup.sql** - Database setup
- **ELASTICSEARCH_DEEP_DIVE.md** - Technical deep dive

---

**✅ ყველაფერი მზადაა გამოსაყენებლად! 🎉**

**შემდეგი ნაბიჯი:** `start-all.bat` → გახსენი http://localhost:5000
@echo off
echo ╔══════════════════════════════════════════════════════════════╗
echo ║     ELASTICSEARCH API - START                                ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

cd ElasticSearch.Api
dotnet run

pause

