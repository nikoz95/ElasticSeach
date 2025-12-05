@echo off
echo Testing Docker build...
echo.

echo Checking Release folder...
if exist "ElasticSeach\bin\Release\net9.0\ElasticSeach.dll" (
    echo ✅ Release build found
    dir /b ElasticSeach\bin\Release\net9.0 | findstr /i "dll"
) else (
    echo ❌ Release build not found. Building now...
    cd ElasticSeach
    dotnet build -c Release
    cd ..
)

echo.
echo Building Docker image...
docker build -t elasticsearch-demo -f Dockerfile . 2>&1

if %errorlevel% equ 0 (
    echo ✅ Docker build successful!
) else (
    echo ❌ Docker build failed!
)

pause

