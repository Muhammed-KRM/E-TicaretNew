@echo off
echo ==========================================
echo Docker Desktop Kontrol
echo ==========================================
echo.
echo Docker durumu kontrol ediliyor...
echo.

docker ps >nul 2>&1

if %errorlevel% equ 0 (
    echo ✅ Docker Desktop çalışıyor!
    echo.
    echo Çalışan container'lar:
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    echo.
    echo Docker ile başlatmak için:
    echo   docker-compose up -d --build
    echo.
) else (
    echo ❌ Docker Desktop çalışmıyor!
    echo.
    echo Lütfen:
    echo   1. Docker Desktop uygulamasını başlatın
    echo   2. Docker'ın tamamen başlamasını bekleyin (30-60 saniye)
    echo   3. Bu script'i tekrar çalıştırın
    echo.
    echo Docker Desktop kurulu değilse:
    echo   https://www.docker.com/products/docker-desktop
    echo.
)

pause
