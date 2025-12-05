@echo off
echo ╔══════════════════════════════════════════════════════════════╗
echo ║     ELASTICSEARCH DEMO - INTERACTIVE MODE                    ║
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

echo 🚀 Ensuring Elasticsearch and Kibana are running...
docker-compose up -d elasticsearch kibana

if %errorlevel% equ 0 (
    echo.
    echo ✅ Services are ready!
    echo.
    echo ⏳ Waiting 10 seconds for services to initialize...
    timeout /t 10 /nobreak >nul
    echo.
    echo 🎮 Starting interactive demo application...
    echo    ^(You can now interact with the menu^)
    echo.
    docker-compose run --rm elasticsearch-demo
) else (
    echo.
    echo ❌ Failed to start services!
    pause
    exit /b 1
)

