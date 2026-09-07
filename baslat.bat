@echo off
setlocal EnableDelayedExpansion

echo ==========================================
echo ETicaret Docker Başlatma
echo ==========================================
echo.

REM Docker kontrolü
echo Docker Desktop kontrolü...
docker ps >nul 2>&1

if %errorlevel% neq 0 (
    echo.
    echo ❌ HATA: Docker Desktop çalışmıyor!
    echo.
    echo Lütfen:
    echo   1. Docker Desktop uygulamasını başlatın
    echo   2. Docker'ın tamamen başlamasını bekleyin (30-60 saniye)
    echo   3. Bu script'i tekrar çalıştırın
    echo.
    echo Kontrol için: docker-kontrol.bat
    echo.
    pause
    exit /b 1
)

echo ✅ Docker Desktop çalışıyor
echo.
echo Docker container'ları başlatılıyor...
echo Bu işlem ilk seferde biraz uzun sürebilir (image'ler indirilecek)
echo.

REM Docker Compose ile başlat
docker-compose up -d --build

if %errorlevel% equ 0 (
    echo.
    echo ==========================================
    echo ✅ Tüm servisler başarıyla başlatıldı!
    echo ==========================================
    echo.
    echo Servisler:
    echo   🌐 Web:          http://localhost:8080
    echo   🔌 API:          http://localhost:5001
    echo   📊 RabbitMQ:     http://localhost:15672
    echo   🔍 PostgreSQL:   localhost:5432
    echo   🗄️  Redis:        localhost:6379
    echo.
    echo Admin Hesabı:
    echo   Email: admin@eticaret.com
    echo   Şifre: Admin123!
    echo.
    echo Container durumunu görmek için:
    echo   docker-compose ps
    echo.
    echo Logları görmek için:
    echo   docker-compose logs -f
    echo.
    echo Durdurmak için:
    echo   durdur.bat veya docker-compose down
    echo.
    echo ==========================================
    echo.
    echo Servisler başlatıldı, hazır olması için 30 saniye bekleniyor...
    timeout /t 30 /nobreak > nul
    echo.
    echo Tarayıcıda açılıyor: http://localhost:8080
    start http://localhost:8080
) else (
    echo.
    echo ❌ HATA: Docker container'ları başlatılamadı!
    echo.
    echo Loglara bakmak için:
    echo   docker-compose logs
    echo.
)

pause