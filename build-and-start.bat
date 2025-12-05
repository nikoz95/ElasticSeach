@echo off
echo ╔══════════════════════════════════════════════════════════════╗
echo ║     ELASTICSEARCH DEMO - BUILD AND START                     ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

echo 1️⃣ Building .NET application locally...
cd ElasticSeach
dotnet build -c Release

if %errorlevel% neq 0 (
    echo ❌ .NET build failed!
    cd ..
    pause
    exit /b 1
)

echo ✅ .NET build successful
echo.

cd ..

echo 2️⃣ Checking if Docker is running...
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo ✅ Docker is running
echo.

echo 3️⃣ Building Docker images...
docker-compose build

if %errorlevel% neq 0 (
    echo ❌ Docker build failed!
    pause
    exit /b 1
)

echo ✅ Docker build successful
echo.

echo 4️⃣ Starting all services...
docker-compose up -d

if %errorlevel% equ 0 (
    echo.
    echo ╔══════════════════════════════════════════════════════════════╗
    echo ║                    ✅ ALL DONE!                              ║
    echo ╚══════════════════════════════════════════════════════════════╝
    echo.
    echo 📊 Services status:
    docker-compose ps
    echo.
    echo 🌐 Access:
    echo    - Elasticsearch: http://localhost:9200
    echo    - Kibana: http://localhost:5601
    echo.
    echo ⏳ Wait 30-60 seconds for services to be ready
    echo.
    echo 💡 Next steps:
    echo    - View logs: docker-compose logs -f
    echo    - Run demo: docker-compose run --rm elasticsearch-demo
    echo    - Stop: stop-docker.bat
    echo.
) else (
    echo ❌ Failed to start services!
    pause
    exit /b 1
)

pause

