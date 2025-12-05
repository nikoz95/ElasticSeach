# 🚀 სწრაფი დაწყება - Elasticsearch Demo Docker-ით

## ნაბიჯ-ნაბიჯ ინსტრუქცია

### 1️⃣ წინაპირობები

**Docker Desktop-ის ინსტალაცია:**

1. გადმოწერეთ Docker Desktop Windows-ისთვის:
   - https://www.docker.com/products/docker-desktop

2. დააინსტალირეთ და გაუშვით Docker Desktop

3. დარწმუნდით რომ Docker მუშაობს:
   - Docker Desktop-ის icon უნდა ჩანდეს system tray-ში
   - icon-ი უნდა იყოს მწვანე (არა წითელი)

### 2️⃣ სისტემის შემოწმება

გაუშვით შემოწმების script:

```cmd
check-system.bat
```

ეს შეამოწმებს:
- ✅ Docker ინსტალირებულია თუ არა
- ✅ Docker გაშვებულია თუ არა  
- ✅ Ports (9200, 5601) თავისუფალია თუ არა
- ✅ საჭირო ფაილები არსებობს თუ არა

### 3️⃣ Build და გაშვება

**🚀 ᲧᲕᲔᲚᲐᲖᲔ ᲛᲐᲠᲢᲘᲕᲘ გზა (რეკომენდებული):**

```cmd
build-and-start.bat
```

ეს script:
1. ✅ ააგებს .NET აპლიკაციას ლოკალურად
2. ✅ შექმნის Docker images-ებს
3. ✅ გაუშვებს ყველა სერვისს (Elasticsearch, Kibana, Demo App)

**ან ხელით ნაბიჯ-ნაბიჯ:**

```cmd
# 1. .NET build
cd ElasticSeach
dotnet build -c Release
cd ..

# 2. Docker images build
docker-compose build

# 3. Start all services
docker-compose up -d
```

### 4️⃣ დაელოდეთ ჩატვირთვას

პირველი გაშვებისას Docker:
1. გადმოწერს images-ებს (2-3 GB) - ეს ერთხელ ხდება
2. დააბილდებს .NET აპლიკაციას
3. გაუშვებს ყველა სერვისს

⏳ **დაელოდეთ 30-60 წამს** სანამ ყველაფერი ჩაიტვირთება.

### 5️⃣ შემოწმება

**Elasticsearch:**
- ბრაუზერში: http://localhost:9200
- უნდა დაინახოთ JSON response

**Kibana:**
- ბრაუზერში: http://localhost:5601
- უნდა ჩაიტვირთოს Kibana UI

**Demo აპლიკაცია:**
```cmd
docker-compose logs elasticsearch-demo
```

### 6️⃣ გამოყენება

#### A. Kibana Dev Tools

1. გახსენით: http://localhost:5601
2. გადადით: Management > Dev Tools Console
3. შეასრულეთ queries:

```json
GET /_cat/indices?v

GET /products/_search
{
  "query": {
    "match_all": {}
  }
}
```

#### B. .NET Demo აპლიკაცია

თუ გაუშვით `run-demo.bat`, ინტერაქტიული მენიუ გაქვთ:

```
1. Index Management
2. Data Types and Mappings
3. Indexing Documents
4. Text Analysis Basics
5. Query DSL Basics
6. Advanced Search Queries
7. Kibana Commands Guide
8. Run Complete Demonstration
0. Exit
```

აირჩიეთ **8** სრული დემონსტრაციისთვის.

#### C. ლოგების ნახვა

```cmd
# ყველა სერვისი
docker-compose logs -f

# მხოლოდ Elasticsearch
docker-compose logs -f elasticsearch

# მხოლოდ Demo აპლიკაცია
docker-compose logs -f elasticsearch-demo
```

### 7️⃣ გაჩერება

```cmd
stop-docker.bat
```

ან

```cmd
docker-compose stop
```

**მონაცემები შენახული იქნება!** შეგიძლიათ ხელახლა გაუშვათ `start-docker.bat` და ყველაფერი დარჩება.

### 8️⃣ სრული წაშლა

თუ გსურთ ყველაფრის წაშლა (მონაცემებთან ერთად):

```cmd
docker-compose down -v
```

---

## 📊 რა უნდა ნახოთ

### Elasticsearch (http://localhost:9200)

```json
{
  "name" : "...",
  "cluster_name" : "docker-cluster",
  "version" : {
    "number" : "8.11.0",
    ...
  },
  "tagline" : "You Know, for Search"
}
```

### Kibana (http://localhost:5601)

Kibana მთავარი გვერდი UI-თი.

### Demo App Logs

```
╔══════════════════════════════════════════════════════════════╗
║     ELASTICSEARCH LEVEL II - DEMONSTRATION PROJECT          ║
╚══════════════════════════════════════════════════════════════╝

🌐 Elasticsearch URL: http://elasticsearch:9200
✓ Connected to Elasticsearch successfully!
...
```

---

## 🐛 პრობლემების გადაჭრა

### ❌ Docker is not running

**პრობლემა:** Docker Desktop არ არის გაშვებული

**გადაწყვეტა:**
1. გაუშვით Docker Desktop
2. დაელოდეთ სანამ icon მწვანე გახდება
3. სცადეთ თავიდან

### ❌ Port already in use

**პრობლემა:** Port 9200 ან 5601 უკვე გამოიყენება

**გადაწყვეტა:**
```cmd
# იპოვეთ რა იყენებს port-ს
netstat -ano | findstr :9200

# გააჩერეთ სხვა Elasticsearch instances
docker ps
docker stop <container-name>
```

### ❌ Elasticsearch crash / Out of memory

**პრობლემა:** არასაკმარისი RAM

**გადაწყვეტა:**
1. Docker Desktop > Settings > Resources
2. Memory: დააყენეთ მინიმუმ 4GB (რეკომენდებული 8GB)
3. Apply & Restart
4. სცადეთ თავიდან: `docker-compose up -d --force-recreate`

### ❌ Cannot connect to Elasticsearch

**პრობლემა:** .NET აპლიკაცია არ უკავშირდება Elasticsearch-ს

**გადაწყვეტა:**
```cmd
# შეამოწმეთ რომ Elasticsearch გაშვებულია
curl http://localhost:9200

# შეამოწმეთ კონტეინერების სტატუსი
docker-compose ps

# რესტარტი
docker-compose restart
```

### 🔄 თავიდან დაწყება (Clean Start)

თუ რაღაც არ მუშაობს:

```cmd
# ყველაფრის წაშლა
docker-compose down -v

# Networks-ის გასუფთავება
docker network prune -f

# Images-ების ხელახალი აგება
docker-compose build --no-cache

# თავიდან გაშვება
docker-compose up -d
```

---

## 📁 შექმნილი ფაილები

```
ElasticSeach/
├── docker-compose.yml          # Docker Compose კონფიგურაცია
├── Dockerfile                  # .NET აპლიკაციის image
├── .dockerignore              # Docker ignore rules
│
├── check-system.bat           # სისტემის შემოწმება
├── start-docker.bat           # სერვისების გაშვება
├── run-demo.bat               # ინტერაქტიული demo
├── stop-docker.bat            # სერვისების გაჩერება
│
├── DOCKER_README.md           # Docker სწრაფი გაიდი (ეს ფაილი)
├── DOCKER_GUIDE.md            # დეტალური Docker ინსტრუქციები
└── README.md                   # მთავარი README
```

---

## 💡 სასარგებლო ბრძანებები

```cmd
# სტატუსის შემოწმება
docker-compose ps

# ლოგები
docker-compose logs -f

# კონკრეტული სერვისის ლოგები
docker-compose logs -f elasticsearch
docker-compose logs -f kibana
docker-compose logs -f elasticsearch-demo

# რესტარტი
docker-compose restart

# გაჩერება (მონაცემების შენახვით)
docker-compose stop

# გაშვება თავიდან
docker-compose start

# სერვისების რესურსების ნახვა
docker stats

# კონტეინერში შესვლა
docker exec -it elasticsearch bash
docker exec -it kibana bash
docker exec -it elasticsearch-demo-app bash

# ყველაფრის წაშლა
docker-compose down -v
```

---

## 🎯 სრული Demo Workflow

1. **გაშვება:**
   ```cmd
   check-system.bat
   start-docker.bat
   ```

2. **დაელოდეთ 1 წუთს** სერვისების ჩატვირთვას

3. **შემოწმება:**
   - Elasticsearch: http://localhost:9200
   - Kibana: http://localhost:5601

4. **Demo App:**
   ```cmd
   run-demo.bat
   # აირჩიეთ: 8 (Run Complete Demonstration)
   ```

5. **Kibana-ში მუშაობა:**
   - Dev Tools: http://localhost:5601/app/dev_tools#/console
   - გაუშვით:
     ```
     GET /products/_search
     GET /articles/_search
     GET /_cat/indices?v
     ```

6. **დასრულება:**
   ```cmd
   stop-docker.bat
   ```

---

**წარმატებები! 🚀**

დეტალური ინფორმაციისთვის იხილეთ [DOCKER_GUIDE.md](DOCKER_GUIDE.md)

