# 📦 Docker Files Summary

## შექმნილი ფაილები

### Core Docker Files

1. **docker-compose.yml**
   - მთავარი კონფიგურაცია 3 სერვისისთვის:
     - `elasticsearch` - Elasticsearch 8.11.0
     - `kibana` - Kibana 8.11.0
     - `elasticsearch-demo` - .NET 9.0 აპლიკაცია
   - კონფიგურაცია:
     - Network: `elastic-network`
     - Volume: `elasticsearch-data` (მონაცემების შენახვისთვის)
     - Health checks ყველა სერვისზე

2. **Dockerfile**
   - Multi-stage build .NET 9.0 აპლიკაციისთვის:
     - Stage 1: Build (SDK 9.0)
     - Stage 2: Publish
     - Stage 3: Runtime (Runtime 9.0)
   - მოიცავს curl-ს health checks-ისთვის

3. **.dockerignore**
   - გამორიცხავს არასაჭირო ფაილებს build-ის დროს:
     - bin/, obj/
     - .git, .vs, .vscode
     - node_modules და სხვა

### Helper Scripts (Windows .bat ფაილები)

4. **check-system.bat**
   - ამოწმებს სისტემის მზადყოფნას:
     - Docker ინსტალაცია
     - Docker Compose
     - Docker running status
     - Ports availability (9200, 5601)
     - Docker ფაილების არსებობა
   - გამოიყენება: პრობლემების დიაგნოსტიკა

5. **start-docker.bat**
   - ავტომატურად იწყებს ყველა სერვისს
   - ამოწმებს Docker-ის სტატუსს
   - აჩვენებს სტატუსს და URLs
   - გამოიყენება: პირველი გაშვება და სწრაფი დაწყება

6. **run-demo.bat**
   - იწყებს Elasticsearch და Kibana-ს background-ში
   - გაუშვებს .NET აპლიკაციას ინტერაქტიულ რეჟიმში
   - საშუალებას გაძლევთ იმუშაოთ მენიუსთან
   - გამოიყენება: ინტერაქტიული დემონსტრაცია

7. **stop-docker.bat**
   - აჩერებს ყველა სერვისს (მონაცემების შენახვით)
   - გამოიყენება: სამუშაოს დასრულება

### Documentation Files

8. **QUICKSTART.md**
   - ნაბიჯ-ნაბიჯ ინსტრუქცია დამწყებთათვის
   - სწრაფი დაწყების გაიდი
   - ხშირი პრობლემების გადაწყვეტა
   - სრული demo workflow

9. **DOCKER_GUIDE.md**
   - დეტალური Docker ინსტრუქციები
   - ყველა ბრძანების აღწერა
   - ბევრი use case და სცენარი
   - დიაგნოსტიკა და troubleshooting
   - Kibana integration გაიდი

10. **DOCKER_README.md**
    - მოკლე სწრაფი მითითებები
    - ძირითადი ბრძანებები
    - Links სხვა დოკუმენტაციაზე

## გამოყენების სცენარები

### Scenario 1: პირველი გაშვება

```cmd
1. check-system.bat         # შემოწმება
2. start-docker.bat         # გაშვება
3. http://localhost:9200    # Elasticsearch
4. http://localhost:5601    # Kibana
```

### Scenario 2: დემონსტრაცია

```cmd
1. run-demo.bat             # ინტერაქტიული რეჟიმი
2. აირჩიეთ 8               # სრული დემო
3. Kibana Dev Tools         # შედეგების ნახვა
```

### Scenario 3: განვითარება

```cmd
1. docker-compose up -d elasticsearch kibana
2. cd ElasticSeach && dotnet run     # ლოკალურად
```

### Scenario 4: პრეზენტაცია

```cmd
1. start-docker.bat         # background-ში
2. http://localhost:5601    # Kibana პრეზენტაციისთვის
3. run-demo.bat             # live coding
```

## Docker Commands Reference

### Basic Operations

```cmd
# გაშვება
docker-compose up -d

# გაჩერება
docker-compose stop

# რესტარტი
docker-compose restart

# ლოგები
docker-compose logs -f

# სტატუსი
docker-compose ps

# წაშლა
docker-compose down -v
```

### Advanced Operations

```cmd
# კონკრეტული სერვისის rebuild
docker-compose build elasticsearch-demo

# Force recreate
docker-compose up -d --force-recreate

# Scale (თუ საჭიროა)
docker-compose up -d --scale elasticsearch-demo=0

# Exec into container
docker exec -it elasticsearch bash
docker exec -it kibana bash
docker exec -it elasticsearch-demo-app bash

# Resources monitoring
docker stats

# Network inspection
docker network inspect elasticsearch_elastic-network
```

## პორტები

| სერვისი | Port | Description |
|---------|------|-------------|
| Elasticsearch | 9200 | HTTP API |
| Elasticsearch | 9300 | Transport (node-to-node) |
| Kibana | 5601 | Web UI |

## Environment Variables

.NET აპლიკაციაში:
- `ELASTICSEARCH_URL` - Elasticsearch URL (default: `http://elasticsearch:9200`)

Docker-ში კონტეინერები ერთმანეთს hostname-ით უკავშირდებიან:
- .NET app -> `http://elasticsearch:9200`
- Kibana -> `http://elasticsearch:9200`
- Host -> `http://localhost:9200`

## Volumes

- `elasticsearch-data` - Elasticsearch მონაცემების persistent storage
  - წაშლა: `docker-compose down -v`
  - შენახვა: `docker-compose stop` ან `docker-compose down` (without -v)

## Health Checks

ყველა სერვისს აქვს health check:

1. **Elasticsearch**: `curl http://localhost:9200/_cluster/health`
2. **Kibana**: `curl http://localhost:5601/api/status`
3. **Demo App**: დამოკიდებულია Elasticsearch და Kibana-ს healthy status-ზე

## Troubleshooting Quick Reference

| პრობლემა | გადაწყვეტა |
|----------|------------|
| Docker not running | გაუშვით Docker Desktop |
| Port in use | `netstat -ano \| findstr :9200` და `docker stop` |
| Out of memory | Docker Desktop > Settings > Resources > Memory: 4GB+ |
| Connection refused | `docker-compose restart` |
| Clean start needed | `docker-compose down -v && docker-compose up -d --build` |

## რესურსების მოთხოვნები

### Minimum
- RAM: 4GB
- Disk: 10GB
- CPU: 2 cores

### Recommended
- RAM: 8GB
- Disk: 20GB
- CPU: 4 cores

## Next Steps

1. **დაწყება**: წაიკითხეთ [QUICKSTART.md](QUICKSTART.md)
2. **დეტალები**: წაიკითხეთ [DOCKER_GUIDE.md](DOCKER_GUIDE.md)
3. **პროექტი**: წაიკითხეთ [README.md](README.md)

---

**შექმნილია: 2024-11-25**
**Version: 1.0**
**Status: ✅ Ready for use**

