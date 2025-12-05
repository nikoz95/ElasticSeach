# 📚 Documentation Index

## 🚀 დაწყება

თუ პირველად იყენებთ ამ პროექტს:

1. **START HERE**: [QUICKSTART.md](QUICKSTART.md)
   - ნაბიჯ-ნაბიჯ ინსტრუქცია Docker-ით გასაშვებად
   - სწრაფი setup და პირველი ტესტი
   - რეკომენდებული ყველასთვის

2. **System Check**: გაუშვით `check-system.bat`
   - შეამოწმებს ყველა წინაპირობას

3. **Quick Start**: გაუშვით `start-docker.bat`
   - დაიწყებს ყველა სერვისს ავტომატურად

## 📖 დოკუმენტაცია

### Docker რესურსები

| ფაილი | დანიშნულება | როდის გამოიყენო |
|-------|-------------|------------------|
| [QUICKSTART.md](QUICKSTART.md) | სწრაფი დაწყების გაიდი | პირველი გაშვება |
| [DOCKER_GUIDE.md](DOCKER_GUIDE.md) | სრული Docker ინსტრუქციები | დეტალური ინფორმაცია |
| [DOCKER_README.md](DOCKER_README.md) | მოკლე მითითებები | სწრაფი დახმარება |
| [DOCKER_FILES_SUMMARY.md](DOCKER_FILES_SUMMARY.md) | ყველა Docker ფაილის აღწერა | ტექნიკური დეტალები |

### პროექტის დოკუმენტაცია

| ფაილი | დანიშნულება | როდის გამოიყენო |
|-------|-------------|------------------|
| [README.md](README.md) | მთავარი დოკუმენტაცია | პროექტის overview |
| [PRESENTATION_SCRIPT.md](PRESENTATION_SCRIPT.md) | პრეზენტაციის სკრიპტი | პრეზენტაციისთვის |
| [PRESENTATION_STRUCTURE.md](PRESENTATION_STRUCTURE.md) | პრეზენტაციის სტრუქტურა | პრეზენტაციის დაგეგმვა |

### Helper Scripts (.bat ფაილები)

| ფაილი | დანიშნულება | გამოყენება |
|-------|-------------|-----------|
| `check-system.bat` | სისტემის შემოწმება | პრობლემების დიაგნოსტიკა |
| `start-docker.bat` | სერვისების გაშვება | პირველი დაწყება |
| `run-demo.bat` | ინტერაქტიული demo | ინტერაქტიული მუშაობა |
| `stop-docker.bat` | სერვისების გაჩერება | დასრულება |

## 🎯 Use Cases

### Use Case 1: პირველი გაშვება

```
1. წაიკითხე: QUICKSTART.md
2. გაუშვი: check-system.bat
3. გაუშვი: start-docker.bat
4. გახსენი: http://localhost:9200 და http://localhost:5601
5. დაბრუნდი: README.md → პროექტის გაცნობა
```

### Use Case 2: Demo/პრეზენტაცია

```
1. წაიკითხე: PRESENTATION_SCRIPT.md
2. გაუშვი: start-docker.bat
3. გაუშვი: run-demo.bat
4. აირჩიე: Option 8 (Complete Demo)
5. გამოიყენე: Kibana Dev Tools
```

### Use Case 3: განვითარება

```
1. წაიკითხე: DOCKER_GUIDE.md
2. გაუშვი: docker-compose up -d elasticsearch kibana
3. ლოკალურად: cd ElasticSeach && dotnet run
4. ცვლილებები: edit code
5. ტესტირება: http://localhost:9200
```

### Use Case 4: პრობლემის გადაჭრა

```
1. გაუშვი: check-system.bat
2. წაიკითხე: DOCKER_GUIDE.md → Troubleshooting section
3. ან: QUICKSTART.md → პრობლემების გადაჭრა
4. Clean start: docker-compose down -v && docker-compose up -d
```

## 📁 ფაილების ორგანიზაცია

```
ElasticSeach/
│
├── 📚 სწრაფი დაწყება
│   ├── DOCUMENTATION_INDEX.md        ← თქვენ აქ ხართ
│   ├── QUICKSTART.md                 ← დაიწყეთ აქედან!
│   ├── DOCKER_README.md              ← სწრაფი მითითებები
│   │
│   ├── check-system.bat              ← პირველი ნაბიჯი
│   ├── start-docker.bat              ← სწრაფი გაშვება
│   ├── run-demo.bat                  ← ინტერაქტიული
│   └── stop-docker.bat               ← გაჩერება
│
├── 📖 დეტალური დოკუმენტაცია
│   ├── README.md                     ← პროექტის მთავარი
│   ├── DOCKER_GUIDE.md               ← Docker დეტალები
│   ├── DOCKER_FILES_SUMMARY.md       ← ტექნიკური დეტალები
│   │
│   ├── PRESENTATION_SCRIPT.md        ← პრეზენტაციის სკრიპტი
│   ├── PRESENTATION_STRUCTURE.md     ← სტრუქტურა
│   ├── FINAL_READY_TO_PRESENT.md
│   ├── FINAL_STATUS.md
│   └── 00_PROJECT_COMPLETE.md
│
├── 🐳 Docker კონფიგურაცია
│   ├── docker-compose.yml            ← მთავარი კონფიგურაცია
│   ├── Dockerfile                    ← .NET image
│   └── .dockerignore                 ← Ignore rules
│
└── 💻 Source Code
    └── ElasticSeach/
        ├── Program.cs
        ├── Models/
        ├── Services/
        └── KibanaGuide/
```

## 🔍 სწრაფი ძებნა

### როგორ...

- **გავუშვა პროექტი?** → [QUICKSTART.md](QUICKSTART.md)
- **დავაინსტალირო Docker?** → [QUICKSTART.md](QUICKSTART.md) → Section 1
- **გავაჩერო სერვისები?** → `stop-docker.bat` ან `docker-compose stop`
- **ვნახო ლოგები?** → `docker-compose logs -f`
- **გავასუფთავო ყველაფერი?** → `docker-compose down -v`
- **ვიმუშაო Kibana-სთან?** → [DOCKER_GUIDE.md](DOCKER_GUIDE.md) → Kibana Section
- **გავიგო პრობლემა?** → `check-system.bat` + [DOCKER_GUIDE.md](DOCKER_GUIDE.md) → Troubleshooting

### თემატური ძებნა

- **Docker Setup**: QUICKSTART.md, DOCKER_GUIDE.md, DOCKER_README.md
- **პრეზენტაცია**: PRESENTATION_SCRIPT.md, PRESENTATION_STRUCTURE.md, README.md
- **ტექნიკური დეტალები**: DOCKER_FILES_SUMMARY.md, DOCKER_GUIDE.md
- **პრობლემები**: QUICKSTART.md (Section 8), DOCKER_GUIDE.md (Troubleshooting)
- **Commands**: DOCKER_README.md, DOCKER_GUIDE.md

## ⚡ Quick Reference

### ძირითადი ბრძანებები

```cmd
# Setup
check-system.bat                    # შემოწმება
start-docker.bat                    # გაშვება
run-demo.bat                        # ინტერაქტიული

# Control
docker-compose ps                   # სტატუსი
docker-compose logs -f              # ლოგები
docker-compose restart              # რესტარტი
stop-docker.bat                     # გაჩერება

# Cleanup
docker-compose down                 # გაჩერება + წაშლა
docker-compose down -v              # + volumes წაშლა
```

### URLs

- **Elasticsearch**: http://localhost:9200
- **Kibana**: http://localhost:5601
- **Kibana Dev Tools**: http://localhost:5601/app/dev_tools#/console

## 💡 რჩევები

1. **დამწყებთათვის**: დაიწყეთ QUICKSTART.md-ით
2. **გამოცდილთათვის**: პირდაპირ `start-docker.bat`
3. **პრობლემის შემთხვევაში**: `check-system.bat` პირველ რიგში
4. **დეტალებისთვის**: DOCKER_GUIDE.md არის comprehensive
5. **პრეზენტაციისთვის**: PRESENTATION_SCRIPT.md + run-demo.bat

## 🆘 დახმარება

თუ ვერ იპოვეთ საჭირო ინფორმაცია:

1. შეამოწმეთ ეს index ფაილი თავიდან
2. გაუშვით `check-system.bat` დიაგნოსტიკისთვის
3. წაიკითხეთ DOCKER_GUIDE.md → Troubleshooting
4. წაიკითხეთ QUICKSTART.md → პრობლემების გადაჭრა

## 📊 სტატუსი

✅ **ყველა ფაილი მზადაა და დატესტილია**

- Docker setup: ✅ სრული
- Documentation: ✅ სრული
- Helper scripts: ✅ სრული
- Examples: ✅ სრული

---

**შექმნილია**: 2024-11-25  
**Version**: 1.0  
**Status**: ✅ Production Ready

**დაიწყეთ აქ**: [QUICKSTART.md](QUICKSTART.md)

