# ✅ Docker Setup - სრული ინსტრუქცია

## 📦 რა შეიქმნა

თქვენი პროექტისთვის შექმნილია სრული Docker infrastructure:

### Docker ფაილები:
- ✅ `docker-compose.yml` - სამი სერვისი (Elasticsearch, Kibana, .NET App)
- ✅ `Dockerfile` - .NET 9 აპლიკაციის image (pre-built binaries approach)
- ✅ `.dockerignore` - Docker build optimization

### Helper Scripts:
- ✅ `build-and-start.bat` - **MAIN SCRIPT** - ყველაფრის build და start
- ✅ `check-system.bat` - სისტემის შემოწმება
- ✅ `start-docker.bat` - სწრაფი გაშვება
- ✅ `run-demo.bat` - ინტერაქტიული demo
- ✅ `stop-docker.bat` - სერვისების გაჩერება
- ✅ `test-build.bat` - build ტესტირება

### დოკუმენტაცია:
- ✅ `QUICKSTART.md` - სწრაფი დაწყების გაიდი
- ✅ `DOCKER_GUIDE.md` - დეტალური Docker ინსტრუქციები
- ✅ `DOCKER_README.md` - სწრაფი მითითებები
- ✅ `DOCKER_FILES_SUMMARY.md` - ყველა ფაილის აღწერა
- ✅ `DOCUMENTATION_INDEX.md` - დოკუმენტაციის ინდექსი
- ✅ `README.md` - განახლებული Docker ინსტრუქციებით

## 🚀 როგორ გამოვიყენოთ

### Option 1: ყველაზე მარტივი (რეკომენდებული)

```cmd
build-and-start.bat
```

ეს **ყველაფერს** გააკეთებს ავტომატურად!

### Option 2: ნაბიჯ-ნაბიჯ

```cmd
# 1. შეამოწმე სისტემა
check-system.bat

# 2. .NET build (IMPORTANT!)
cd ElasticSeach
dotnet build -c Release
cd ..

# 3. Docker build
docker-compose build

# 4. Start
docker-compose up -d
```

### Option 3: ინტერაქტიული Demo

```cmd
# 1. Build (თუ არ გაქვს)
build-and-start.bat

# 2. Stop demo app
docker-compose stop elasticsearch-demo

# 3. Run interactively
docker-compose run --rm elasticsearch-demo
```

## ⚠️ მნიშვნელოვანი

### SSL/NuGet პრობლემის გადაწყვეტა

Docker-ში NuGet SSL პრობლემების გამო, გამოვიყენეთ **pre-built binaries** მიდგომა:

1. ✅ .NET აპლიკაცია იბილდება **ლოკალურად** (Windows-ზე)
2. ✅ Release binaries კოპირდება Docker image-ში
3. ✅ არ არის საჭირო NuGet restore Docker-ში

ამიტომ **აუცილებელია**:
```cmd
cd ElasticSeach
dotnet build -c Release
cd ..
```

გაშვებამდე!

## 📁 სტრუქტურა

```
ElasticSeach/
├── docker-compose.yml          # 3 სერვისი
├── Dockerfile                  # .NET Runtime + pre-built DLLs
├── .dockerignore               # არ ბლოკავს bin/ ფოლდერს
│
├── build-and-start.bat         # ⭐ MAIN - ყველაფრის გამშვები
├── check-system.bat            # სისტემის შემოწმება
├── start-docker.bat            # სწრაფი start
├── run-demo.bat                # ინტერაქტიული
├── stop-docker.bat             # გაჩერება
├── test-build.bat              # build test
│
├── QUICKSTART.md               # 📖 დაწყება აქედან
├── DOCKER_GUIDE.md             # 📚 დეტალური გაიდი
├── DOCKER_README.md            # 📝 სწრაფი ref
├── DOCKER_FILES_SUMMARY.md     # 📋 ფაილების აღწერა
├── DOCUMENTATION_INDEX.md      # 🗂️ ინდექსი
├── README.md                   # 📖 მთავარი (განახლებული)
│
└── ElasticSeach/
    ├── bin/Release/net9.0/     # ⚠️ საჭიროა Docker-ისთვის!
    └── ...
```

## 🎯 სწრაფი კომანდები

```cmd
# Build & Start
build-and-start.bat

# სტატუსი
docker-compose ps

# ლოგები
docker-compose logs -f
docker-compose logs -f elasticsearch
docker-compose logs -f kibana
docker-compose logs -f elasticsearch-demo

# რესტარტი
docker-compose restart

# გაჩერება
stop-docker.bat
# ან
docker-compose stop

# სრული წაშლა
docker-compose down -v

# ინტერაქტიული demo
docker-compose run --rm elasticsearch-demo
```

## 🌐 URLs

- **Elasticsearch**: http://localhost:9200
- **Kibana**: http://localhost:5601
- **Kibana Dev Tools**: http://localhost:5601/app/dev_tools#/console

## ✅ Checklist გაშვებისთვის

- [ ] Docker Desktop დაინსტალირებული და გაშვებული
- [ ] `check-system.bat` - ყველა ✅
- [ ] .NET build Release-ში (`dotnet build -c Release`)
- [ ] `build-and-start.bat` ან `docker-compose build`
- [ ] `docker-compose up -d`
- [ ] http://localhost:9200 - მუშაობს
- [ ] http://localhost:5601 - მუშაობს

## 🐛 პრობლემების გადაჭრა

### პრობლემა: Docker build fails "not found"

**გადაწყვეტა:**
```cmd
cd ElasticSeach
dotnet build -c Release
cd ..
docker-compose build
```

### პრობლემა: NuGet SSL errors

**გადაწყვეტა:** ეს გადაწყვეტილია pre-built approach-ით. უბრალოდ build-ე ლოკალურად.

### პრობლემა: Port already in use

**გადაწყვეტა:**
```cmd
netstat -ano | findstr :9200
# kill process or
docker-compose down
```

### პრობლემა: Docker not running

**გადაწყვეტა:** გაუშვი Docker Desktop

### Clean start

```cmd
docker-compose down -v
docker system prune -f
cd ElasticSeach
dotnet clean
dotnet build -c Release
cd ..
docker-compose build --no-cache
docker-compose up -d
```

## 📖 სად გავაგრძელო?

1. **პირველად?** → [QUICKSTART.md](QUICKSTART.md)
2. **დეტალები?** → [DOCKER_GUIDE.md](DOCKER_GUIDE.md)
3. **პრობლემა?** → `check-system.bat` + ზემოთ troubleshooting
4. **ყველა დოკი?** → [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)

## 🎉 შედეგი

მას შემდეგ რაც გაუშვებთ `build-and-start.bat`:

1. ✅ Elasticsearch გაშვებული (9200)
2. ✅ Kibana გაშვებული (5601)
3. ✅ .NET Demo App მზადაა (container-ში)
4. ✅ შეგიძლიათ:
   - Kibana Dev Tools-ში queries გაშვება
   - `docker-compose run --rm elasticsearch-demo` - ინტერაქტიული demo
   - Kibana Discover-ში მონაცემების ნახვა

---

**✅ ყველაფერი მზადაა გასაშვებად!**

**დაიწყე აქედან:** `build-and-start.bat` ან [QUICKSTART.md](QUICKSTART.md)

