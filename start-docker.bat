@echo off
echo ╔══════════════════════════════════════════════════════════════╗
echo ║     ELASTICSEARCH DEMO - DOCKER QUICK START                 ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo ✅ Docker is running
echo.

echo 🚀 Starting all services...
echo    - Elasticsearch will be on http://localhost:9200
echo    - Kibana will be on http://localhost:5601
echo.

docker-compose up -d

if %errorlevel% equ 0 (
    echo.
    echo ✅ All services started successfully!
    echo.
    echo 📊 Checking services status...
    docker-compose ps
    echo.
    echo 🌐 Services:
    echo    - Elasticsearch: http://localhost:9200
    echo    - Kibana: http://localhost:5601
    echo.
    echo ⏳ Waiting for services to be ready ^(this may take 30-60 seconds^)...
    timeout /t 5 /nobreak >nul
    echo.
    echo 💡 Tips:
    echo    - View logs: docker-compose logs -f
    echo    - Run demo app interactively: docker-compose run --rm elasticsearch-demo
    echo    - Stop services: docker-compose stop
    echo    - Remove everything: docker-compose down -v
    echo.
    echo 📖 For detailed instructions, see DOCKER_GUIDE.md
    echo.
) else (
    echo.
    echo ❌ Failed to start services!
    echo Check the error messages above and try again.
    echo.
)

pause

