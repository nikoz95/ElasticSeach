# 🚀 Quick Start Guide

## Docker Setup (გაშვება 3 ნაბიჯში)

### 1️⃣ გაუშვით Docker Desktop

### 2️⃣ გაუშვით `docker-start.bat` 
ან terminal-ში:
```bash
docker-compose up -d
```

### 3️⃣ მოითმინეთ 30-60 წამი სერვისების ჩატვირთვას

---

## ✅ რა მოხდება ავტომატურად:

1. ✅ SQL Server ჩაიტვირთება
2. ✅ Elasticsearch და Kibana გაეშვება  
3. ✅ Database შეიქმნება (`ElasticsearchDemo`)
4. ✅ Products table შეიქმნება
5. ✅ 15 სატესტო პროდუქტი ჩაიტვირთება
6. ✅ API გაეშვება (http://localhost:5000)
7. ✅ Background Jobs დაიწყებს სინქრონიზაციას

---

## 🌐 Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| **API** | http://localhost:5000 | - |
| **Kibana** | http://localhost:5601 | - |
| **Elasticsearch** | http://localhost:9200 | - |
| **SQL Server** | `localhost,1433` | sa / YourStrong@Password123 |

---

## 🧪 ტესტირება

### 1. შეამოწმეთ API
```bash
curl http://localhost:5000/api/products/search?query=laptop
```

### 2. შეამოწმეთ Elasticsearch
```bash
curl http://localhost:9200/products/_search
```

### 3. Kibana-ში ძებნა
1. გახსენით: http://localhost:5601
2. Dev Tools → Console
3. გაუშვით:
```json
GET /products/_search
{
  "query": {
    "match_all": {}
  }
}
```

---

## 📊 Logs-ების ნახვა

### All Services
```bash
docker-compose logs -f
```

### Specific Service
```bash
docker-compose logs -f api
docker-compose logs -f jobs
```

ან გაუშვით: `docker-logs.bat`

---

## 🛑 გაჩერება

```bash
docker-compose down
```

ან გაუშვით: `docker-stop.bat`

---

## 🔄 Clean Restart (ყველაფრის წაშლა და თავიდან)

```bash
docker-compose down -v
docker-compose up -d
```

---

## 🎯 რა სინქრონიზდება?

- **ყოველ 5 წუთში** - Incremental Sync (მხოლოდ ცვლილებები)
- **ყოველდღე 2:00 AM** - Full Sync
- **ყოველ კვირას 3:00 AM** - Weekly Full Sync

---

## 💡 Tips

### SQL-ში ცვლილების გაკეთება
```sql
-- Connect to: localhost,1433 (sa/YourStrong@Password123)

-- Update product
UPDATE Products 
SET Price = 9999.99, UpdatedAt = GETDATE() 
WHERE Id = 1;

-- მომდევნო 5 წუთში ავტომატურად გადავა Elasticsearch-ში
```

### Check Database
```bash
# Use your password from .env file
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123

1> USE ElasticsearchDemo;
2> GO
1> SELECT COUNT(*) FROM Products;
2> GO
```

> 💡 პაროლების კონფიგურაციის შესახებ დეტალურად: [ENV-CONFIG.md](ENV-CONFIG.md)

---

## ❓ პრობლემები?

### Container არ ეშვება?
```bash
docker-compose ps
docker-compose logs [service-name]
```

### Port უკვე დაკავებულია?
`docker-compose.yml`-ში შეცვალეთ ports:
```yaml
ports:
  - "5001:8080"  # ნაცვლად 5000:8080
```

### Memory error?
Docker Desktop → Settings → Resources → გაზარდეთ Memory (8GB+ რეკომენდირებული)

---

## 📚 სრული დოკუმენტაცია

იხილეთ: [README.md](README.md)
# 🚀 Quick Start Guide

## Docker Setup (გაშვება 3 ნაბიჯში)

### 1️⃣ გაუშვით Docker Desktop

### 2️⃣ გაუშვით `docker-start.bat` 
ან terminal-ში:
```bash
docker-compose up -d
```

### 3️⃣ მოითმინეთ 30-60 წამი სერვისების ჩატვირთვას

---

## ✅ რა მოხდება ავტომატურად:

1. ✅ SQL Server ჩაიტვირთება
2. ✅ Elasticsearch და Kibana გაეშვება  
3. ✅ Database შეიქმნება (`ElasticsearchDemo`)
4. ✅ Products table შეიქმნება
5. ✅ 15 სატესტო პროდუქტი ჩაიტვირთება
6. ✅ API გაეშვება (http://localhost:5000)
7. ✅ Background Jobs დაიწყებს სინქრონიზაციას

---

## 🌐 Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| **API** | http://localhost:5000 | - |
| **Kibana** | http://localhost:5601 | - |
| **Elasticsearch** | http://localhost:9200 | - |
| **SQL Server** | `localhost,1433` | sa / იხილეთ `.env` (default: YourStrong@Password123) |

---

## 🧪 ტესტირება

### 1. შეამოწმეთ API
```bash
curl http://localhost:5000/api/products/search?query=laptop
```

### 2. შეამოწმეთ Elasticsearch
```bash
curl http://localhost:9200/products/_search
```

### 3. Kibana-ში ძებნა
1. გახსენით: http://localhost:5601
2. Dev Tools → Console
3. გაუშვით:
```json
GET /products/_search
{
  "query": {
    "match_all": {}
  }
}
```

---

## 📊 Logs-ების ნახვა

### All Services
```bash
docker-compose logs -f
```

### Specific Service
```bash
docker-compose logs -f api
docker-compose logs -f jobs
```

ან გაუშვით: `docker-logs.bat`

---

## 🛑 გაჩერება

```bash
docker-compose down
```

ან გაუშვით: `docker-stop.bat`

---

## 🔄 Clean Restart (ყველაფრის წაშლა და თავიდან)

```bash
docker-compose down -v
docker-compose up -d
```

---

## 🎯 რა სინქრონიზდება?

- **ყოველ 5 წუთში** - Incremental Sync (მხოლოდ ცვლილებები)
- **ყოველდღე 2:00 AM** - Full Sync
- **ყოველ კვირას 3:00 AM** - Weekly Full Sync

---

## 💡 Tips

### SQL-ში ცვლილების გაკეთება
```sql
-- Connect to: localhost,1433 (sa/YourStrong@Password123)

-- Update product
UPDATE Products 
SET Price = 9999.99, UpdatedAt = GETDATE() 
WHERE Id = 1;

-- მომდევნო 5 წუთში ავტომატურად გადავა Elasticsearch-ში
```

### Check Database
```bash
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123

1> USE ElasticsearchDemo;
2> GO
1> SELECT COUNT(*) FROM Products;
2> GO
```

---

## ❓ პრობლემები?

### Container არ ეშვება?
```bash
docker-compose ps
docker-compose logs [service-name]
```

### Port უკვე დაკავებულია?
`docker-compose.yml`-ში შეცვალეთ ports:
```yaml
ports:
  - "5001:8080"  # ნაცვლად 5000:8080
```

### Memory error?
Docker Desktop → Settings → Resources → გაზარდეთ Memory (8GB+ რეკომენდირებული)

---

## 📚 სრული დოკუმენტაცია

იხილეთ: [README.md](README.md)
# 🚀 Quick Start Guide

## Docker Setup (გაშვება 3 ნაბიჯში)

### 1️⃣ გაუშვით Docker Desktop

### 2️⃣ (ოფშენალური) შექმენით `.env` ფაილი custom პაროლებისთვის
```bash
copy .env.example .env
notepad .env
```

**შეცვალეთ პაროლი:**
```env
SA_PASSWORD=YourCustomPassword123!
```

> 💡 თუ არ შექმნით `.env` ფაილს, გამოიყენება default პაროლი: `YourStrong@Password123`

### 3️⃣ გაუშვით `docker-start.bat` 
ან terminal-ში:
```bash
docker-compose up -d
```

### 4️⃣ მოითმინეთ 30-60 წამი სერვისების ჩატვირთვას

---

## ✅ რა მოხდება ავტომატურად:

1. ✅ SQL Server ჩაიტვირთება
2. ✅ Elasticsearch და Kibana გაეშვება  
3. ✅ Database შეიქმნება (`ElasticsearchDemo`)
4. ✅ Products table შეიქმნება
5. ✅ 15 სატესტო პროდუქტი ჩაიტვირთება
6. ✅ API გაეშვება (http://localhost:5000)
7. ✅ Background Jobs დაიწყებს სინქრონიზაციას

---

## 🌐 Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| **API** | http://localhost:5000 | - |
| **Kibana** | http://localhost:5601 | - |
| **Elasticsearch** | http://localhost:9200 | - |
| **SQL Server** | `localhost,1433` | sa / YourStrong@Password123 |

---

## 🧪 ტესტირება

### 1. შეამოწმეთ API
```bash
curl http://localhost:5000/api/products/search?query=laptop
```

### 2. შეამოწმეთ Elasticsearch
```bash
curl http://localhost:9200/products/_search
```

### 3. Kibana-ში ძებნა
1. გახსენით: http://localhost:5601
2. Dev Tools → Console
3. გაუშვით:
```json
GET /products/_search
{
  "query": {
    "match_all": {}
  }
}
```

---

## 📊 Logs-ების ნახვა

### All Services
```bash
docker-compose logs -f
```

### Specific Service
```bash
docker-compose logs -f api
docker-compose logs -f jobs
```

ან გაუშვით: `docker-logs.bat`

---

## 🛑 გაჩერება

```bash
docker-compose down
```

ან გაუშვით: `docker-stop.bat`

---

## 🔄 Clean Restart (ყველაფრის წაშლა და თავიდან)

```bash
docker-compose down -v
docker-compose up -d
```

---

## 🎯 რა სინქრონიზდება?

- **ყოველ 5 წუთში** - Incremental Sync (მხოლოდ ცვლილებები)
- **ყოველდღე 2:00 AM** - Full Sync
- **ყოველ კვირას 3:00 AM** - Weekly Full Sync

---

## 💡 Tips

### SQL-ში ცვლილების გაკეთება
```sql
-- Connect to: localhost,1433 (sa/YourStrong@Password123)

-- Update product
UPDATE Products 
SET Price = 9999.99, UpdatedAt = GETDATE() 
WHERE Id = 1;

-- მომდევნო 5 წუთში ავტომატურად გადავა Elasticsearch-ში
```

### Check Database
```bash
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123

1> USE ElasticsearchDemo;
2> GO
1> SELECT COUNT(*) FROM Products;
2> GO
```

---

## ❓ პრობლემები?

### Container არ ეშვება?
```bash
docker-compose ps
docker-compose logs [service-name]
```

### Port უკვე დაკავებულია?
`docker-compose.yml`-ში შეცვალეთ ports:
```yaml
ports:
  - "5001:8080"  # ნაცვლად 5000:8080
```

### Memory error?
Docker Desktop → Settings → Resources → გაზარდეთ Memory (8GB+ რეკომენდირებული)

---

## 📚 სრული დოკუმენტაცია

იხილეთ: [README.md](README.md)

