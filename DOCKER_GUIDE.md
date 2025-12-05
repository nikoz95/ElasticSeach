# 🐳 Docker Setup Guide - Elasticsearch Demo

ეს გაიდი ხსნის როგორ გაუშვათ Elasticsearch, Kibana და .NET აპლიკაცია Docker-ით.

## 📋 წინაპირობები

1. **Docker Desktop** (Windows-ისთვის)
   - გადმოწერეთ: https://www.docker.com/products/docker-desktop
   - დააინსტალირეთ და დარწმუნდით რომ Docker Desktop გაშვებულია
   - შეამოწმეთ ინსტალაცია:
     ```cmd
     docker --version
     docker-compose --version
     ```

2. **საკმარისი რესურსები**
   - RAM: მინიმუმ 4GB (რეკომენდებული 8GB)
   - Disk: მინიმუმ 10GB თავისუფალი სივრცე

## 🚀 სწრაფი დაწყება

### 1. ყველა სერვისის გაშვება

პროექტის root დირექტორიაში გაუშვით:

```cmd
docker-compose up -d
```

ეს კომანდა:
- ✅ დაიწყებს Elasticsearch-ს (port 9200)
- ✅ დაიწყებს Kibana-ს (port 5601)
- ✅ დააბილდებს და გაუშვებს .NET აპლიკაციას

### 2. სტატუსის შემოწმება

```cmd
docker-compose ps
```

ან

```cmd
docker ps
```

დარწმუნდით რომ 3 კონტეინერი გაშვებულია:
- `elasticsearch` - Elasticsearch სერვერი
- `kibana` - Kibana UI
- `elasticsearch-demo-app` - .NET აპლიკაცია

### 3. ლოგების ნახვა

```cmd
# ყველა სერვისის ლოგები
docker-compose logs -f

# მხოლოდ Elasticsearch
docker-compose logs -f elasticsearch

# მხოლოდ Kibana
docker-compose logs -f kibana

# მხოლოდ .NET აპლიკაცია
docker-compose logs -f elasticsearch-demo
```

### 4. სერვისების შემოწმება

**Elasticsearch:**
```cmd
curl http://localhost:9200
```

ან ბრაუზერში: http://localhost:9200

**Kibana:**

ბრაუზერში: http://localhost:5601

დაელოდეთ რამდენიმე წამს სანამ Kibana სრულად ჩაიტვირთება.

### 5. .NET აპლიკაციასთან ინტერაქცია

```cmd
# აპლიკაციის კონტეინერში შესვლა
docker exec -it elasticsearch-demo-app bash

# ან თუ bash არ მუშაობს
docker exec -it elasticsearch-demo-app sh
```

ან აპლიკაციის ლოგებში იხილეთ რა ხდება:

```cmd
docker-compose logs -f elasticsearch-demo
```

თუ აპლიკაცია გაჩერდა, გადაუშვით:

```cmd
docker-compose restart elasticsearch-demo
```

### 6. აპლიკაციის ხელახალი გაშვება ინტერაქტიულ რეჟიმში

თუ გსურთ თამაში აპლიკაციასთან:

```cmd
# პირველ რიგში გააჩერეთ არსებული კონტეინერი
docker-compose stop elasticsearch-demo

# შემდეგ გაუშვით ინტერაქტიულად
docker-compose run --rm elasticsearch-demo
```

ასე შეძლებთ მენიუდან არჩევანის გაკეთებას და პროგრამასთან ურთიერთობას.

## 🛠️ ძირითადი ბრძანებები

### გაშვება და გაჩერება

```cmd
# ყველა სერვისის გაშვება (background)
docker-compose up -d

# ყველა სერვისის გაშვება (foreground - ლოგები ჩანს)
docker-compose up

# ყველა სერვისის გაჩერება
docker-compose stop

# ყველა სერვისის გაჩერება და წაშლა
docker-compose down

# ყველაფრის წაშლა მონაცემებთან ერთად
docker-compose down -v
```

### გადატვირთვა

```cmd
# ყველა სერვისი
docker-compose restart

# კონკრეტული სერვისი
docker-compose restart elasticsearch
docker-compose restart kibana
docker-compose restart elasticsearch-demo
```

### აგება და ხელახალი აგება

```cmd
# .NET აპლიკაციის ხელახალი აგება
docker-compose build elasticsearch-demo

# ხელახალი აგება და გაშვება
docker-compose up -d --build elasticsearch-demo

# ყველაფრის ხელახალი აგება
docker-compose build --no-cache
```

## 🔍 დიაგნოსტიკა

### პრობლემა: Elasticsearch არ იწყება

```cmd
# ლოგების ნახვა
docker-compose logs elasticsearch

# ყველაზე ხშირი პრობლემა - არასაკმარისი მეხსიერება
# გადაწყვეტა: Docker Desktop Settings > Resources > Memory: 4GB+
```

### პრობლემა: Kibana არ იწყება

```cmd
# დარწმუნდით რომ Elasticsearch გაშვებულია
curl http://localhost:9200

# Kibana ლოგები
docker-compose logs kibana

# თუ საჭიროა, რესტარტი
docker-compose restart kibana
```

### პრობლემა: .NET აპლიკაცია არ უკავშირდება Elasticsearch-ს

```cmd
# შეამოწმეთ ქსელი
docker network ls
docker network inspect elasticsearch_elastic-network

# შეამოწმეთ environment ცვლადები
docker exec elasticsearch-demo-app env | grep ELASTICSEARCH
```

### პრობლემა: Port უკვე დაკავებულია

```cmd
# იპოვეთ რა იყენებს port-ს
netstat -ano | findstr :9200
netstat -ano | findstr :5601

# გააჩერეთ Docker კონტეინერები
docker-compose down

# ან შეცვალეთ ports docker-compose.yml-ში
```

## 📊 Kibana გამოყენება

1. **გახსენით Kibana**: http://localhost:5601

2. **Dev Tools Console**:
   - გადადით: Management > Dev Tools
   - ან პირდაპირ: http://localhost:5601/app/dev_tools#/console

3. **Discover**:
   - პირველ რიგში შექმენით Index Pattern
   - გადადით: Management > Stack Management > Index Patterns
   - შექმენით pattern: `products*` ან `articles*`
   - შემდეგ გადადით Analytics > Discover

4. **მონაცემების ნახვა**:
   ```
   # Dev Tools-ში
   GET /products/_search
   {
     "query": {
       "match_all": {}
     }
   }
   ```

## 🧪 ტესტირება

### Elasticsearch Health Check

```cmd
curl http://localhost:9200/_cluster/health?pretty
```

### ინდექსების სია

```cmd
curl http://localhost:9200/_cat/indices?v
```

### Kibana Status

```cmd
curl http://localhost:5601/api/status
```

### .NET აპლიკაციის ტესტი

```cmd
# ლოგების ნახვა
docker-compose logs elasticsearch-demo | more

# კონტეინერში შესვლა
docker exec -it elasticsearch-demo-app bash

# აპლიკაციის ხელახალი გაშვება
docker-compose restart elasticsearch-demo
```

## 📁 სტრუქტურა

```
ElasticSeach/
├── docker-compose.yml          # Docker Compose კონფიგურაცია
├── Dockerfile                  # .NET აპლიკაციის Docker image
├── .dockerignore              # ფაილები რომლებიც არ უნდა დაკოპირდეს
├── DOCKER_GUIDE.md            # ეს ფაილი
└── ElasticSeach/              # .NET პროექტი
    ├── Program.cs
    ├── Models/
    ├── Services/
    └── ...
```

## 🎯 სრული Demo სცენარი

### 1. გაშვება
```cmd
cd C:\Users\Nmalidze\RiderProjects\ElasticSeach
docker-compose up -d
```

### 2. დაელოდეთ სერვისებს (30-60 წამი)
```cmd
docker-compose logs -f
# დააჭირეთ Ctrl+C როცა დაინახავთ "Kibana is now available"
```

### 3. შეამოწმეთ სერვისები
- Elasticsearch: http://localhost:9200
- Kibana: http://localhost:5601

### 4. გაუშვით Demo აპლიკაცია ინტერაქტიულად
```cmd
docker-compose run --rm elasticsearch-demo
```

### 5. აირჩიეთ მენიუდან (მაგ: 8 - სრული დემო)

### 6. გადადით Kibana-ში და ნახეთ მონაცემები
- გახსენით Dev Tools Console
- გაუშვით:
  ```
  GET /products/_search
  GET /articles/_search
  GET /_cat/indices?v
  ```

### 7. გამოიყენეთ Discover
- შექმენით Index Patterns: `products*`, `articles*`
- გადადით Discover და ექსპერიმენტირეთ KQL queries-ით

### 8. დასრულებისას
```cmd
# სერვისების გაჩერება (მონაცემები შენახულია)
docker-compose stop

# ან ყველაფრის წაშლა
docker-compose down -v
```

## 💡 Best Practices

1. **მეხსიერების მართვა**:
   - Docker Desktop-ში გამოყავით საკმარისი RAM (4-8GB)
   - შეამოწმეთ: Docker Desktop > Settings > Resources

2. **მონაცემების შენახვა**:
   - `docker-compose down` - შეინარჩუნებს volumes (მონაცემებს)
   - `docker-compose down -v` - წაშლის volumes-ს (სუფთა დაწყება)

3. **განვითარების რეჟიმი**:
   ```cmd
   # აპლიკაციის კოდის შეცვლის შემდეგ
   docker-compose build elasticsearch-demo
   docker-compose up -d elasticsearch-demo
   ```

4. **Logs Monitoring**:
   ```cmd
   # Real-time ლოგები
   docker-compose logs -f --tail=100
   ```

## 🐛 Troubleshooting

### რესურსების პრობლემები
```cmd
# შეამოწმეთ Docker რესურსების გამოყენება
docker stats

# თუ Elasticsearch crash-დება
# Docker Desktop > Settings > Resources > Memory: 4GB+
```

### Network პრობლემები
```cmd
# ქსელის პრობლემების შემთხვევაში
docker-compose down
docker network prune
docker-compose up -d
```

### წაშლეთ ყველაფერი და თავიდან დაიწყეთ
```cmd
# კონტეინერები, volumes, networks
docker-compose down -v

# გაშვება თავიდან
docker-compose up -d --build
```

## 📞 დახმარება

თუ პრობლემა გაქვთ:

1. **ლოგების შემოწმება**: `docker-compose logs -f`
2. **Docker Desktop Status**: დარწმუნდით რომ Docker Desktop გაშვებულია
3. **Ports**: 9200 და 5601 უნდა იყოს თავისუფალი
4. **Resources**: საკმარისი RAM და Disk Space

---

**წარმატებები Docker-ით! 🐳🎉**

