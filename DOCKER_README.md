# 🐳 Docker Quick Start

## მარტივი გაშვება

### Option 1: ბატ ფაილით (უმარტივესი)

```cmd
start-docker.bat
```

ეს გაუშვებს Elasticsearch-ს, Kibana-ს და .NET აპლიკაციას background-ში.

### Option 2: ინტერაქტიული რეჟიმი

```cmd
run-demo.bat
```

ეს გაუშვებს აპლიკაციას ინტერაქტიულ რეჟიმში, სადაც შეძლებთ მენიუდან არჩევანის გაკეთებას.

### Option 3: ხელით

```cmd
# ყველა სერვისის გაშვება
docker-compose up -d

# აპლიკაციის გაშვება ინტერაქტიულად
docker-compose run --rm elasticsearch-demo
```

## გაჩერება

```cmd
stop-docker.bat
```

ან

```cmd
docker-compose stop
```

## სერვისები

მას შემდეგ რაც გაუშვებთ:

- **Elasticsearch**: http://localhost:9200
- **Kibana**: http://localhost:5601

## დეტალური გაიდი

იხილეთ [DOCKER_GUIDE.md](DOCKER_GUIDE.md) სრული ინსტრუქციებისთვის.

## ხშირი ბრძანებები

```cmd
# ლოგების ნახვა
docker-compose logs -f

# სტატუსის შემოწმება
docker-compose ps

# სერვისების გადატვირთვა
docker-compose restart

# ყველაფრის წაშლა
docker-compose down -v
```

