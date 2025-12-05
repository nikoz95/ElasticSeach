@echo off
setlocal enabledelayedexpansion

echo ╔══════════════════════════════════════════════════════════════╗
echo ║     ELASTICSEARCH DEMO - SYSTEM CHECK                        ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

echo 1. Checking Docker installation...
where docker >nul 2>&1
if %errorlevel% equ 0 (
    echo    ✅ Docker CLI is installed
    docker --version
) else (
    echo    ❌ Docker CLI not found
    echo    Please install Docker Desktop: https://www.docker.com/products/docker-desktop
    goto :error
)
echo.

echo 2. Checking Docker Compose...
where docker-compose >nul 2>&1
if %errorlevel% equ 0 (
    echo    ✅ Docker Compose is installed
    docker-compose --version
) else (
    echo    ℹ Checking if docker compose ^(v2^) is available...
    docker compose version >nul 2>&1
    if !errorlevel! equ 0 (
        echo    ✅ Docker Compose V2 is available
        docker compose version
    ) else (
        echo    ❌ Docker Compose not found
        goto :error
    )
)
echo.

echo 3. Checking if Docker is running...
docker info >nul 2>&1
if %errorlevel% equ 0 (
    echo    ✅ Docker is running
) else (
    echo    ❌ Docker is not running
    echo    Please start Docker Desktop and try again
    goto :error
)
echo.

echo 4. Checking required ports...
netstat -ano | findstr ":9200" >nul 2>&1
if %errorlevel% equ 0 (
    echo    ⚠ Port 9200 is already in use
    echo    You may need to stop other Elasticsearch instances
) else (
    echo    ✅ Port 9200 is available
)

netstat -ano | findstr ":5601" >nul 2>&1
if %errorlevel% equ 0 (
    echo    ⚠ Port 5601 is already in use
    echo    You may need to stop other Kibana instances
) else (
    echo    ✅ Port 5601 is available
)
echo.

echo 5. Checking Docker files...
if exist "docker-compose.yml" (
    echo    ✅ docker-compose.yml found
) else (
    echo    ❌ docker-compose.yml not found
    goto :error
)

if exist "Dockerfile" (
    echo    ✅ Dockerfile found
) else (
    echo    ❌ Dockerfile not found
    goto :error
)
echo.

echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ✅ ALL CHECKS PASSED!                     ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo You can now run:
echo    - start-docker.bat     ^(to start all services^)
echo    - run-demo.bat         ^(to run interactive demo^)
echo.
goto :end

:error
echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                    ❌ CHECKS FAILED!                         ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo Please fix the issues above and try again.
echo.

:end
pause

