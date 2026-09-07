@echo off
setlocal EnableDelayedExpansion

echo ==========================================
echo ETicaret Development Başlatma (Hibrit)
echo ==========================================
echo.
echo Bu yöntem:
echo   ✅ PostgreSQL, Redis, RabbitMQ → Docker'da
echo   ✅ API ve Web → Normal dotnet run (HOT RELOAD!)
echo.
echo Avantajlar:
echo   - Her değişiklik anında görünür
echo   - CSS anında yüklenir
echo   - Yeni sayfalar hemen çalışır
echo   - PostgreSQL kurmanıza gerek yok
echo.

REM Docker kontrolü
echo Docker Desktop kontrolü...
docker ps >nul 2>&1

if %errorlevel% neq 0 (
    echo.
    echo ❌ HATA: Docker Desktop çalışmıyor!
    echo.
    echo Lütfen Docker Desktop'ı başlatın ve tekrar deneyin.
    pause
    exit /b 1
)

echo ✅ Docker Desktop çalışıyor
echo.

REM Tam Docker varsa durdur
echo Tam Docker container'ları durduruluyor (varsa)...
docker-compose down >nul 2>&1

echo.
echo Development servisleri başlatılıyor (PostgreSQL, Redis, RabbitMQ)...
docker-compose -f docker-compose.dev.yml up -d

if %errorlevel% neq 0 (
    echo.
    echo ❌ Docker servisleri başlatılamadı!
    pause
    exit /b 1
)

echo ✅ Docker servisleri başlatıldı
echo.
echo   🔍 PostgreSQL: localhost:5432
echo   🗄️  Redis:      localhost:6379
echo   🐰 RabbitMQ:    localhost:15672
echo.

REM 5 saniye bekle
echo PostgreSQL'in hazır olması için 5 saniye bekleniyor...
timeout /t 5 /nobreak > nul

REM Migration kontrolü
echo.
echo Migration kontrolü yapılıyor...
cd src\ETicaret.API
dotnet ef database update --no-build >nul 2>&1

if %errorlevel% neq 0 (
    echo ⚠️  Migration gerekli, çalıştırılıyor...
    dotnet ef database update
    if %errorlevel% neq 0 (
        echo.
        echo ❌ Migration başarısız! Manuel çalıştırın:
        echo    cd src\ETicaret.API
        echo    dotnet ef database update
        cd ..\..
        pause
        exit /b 1
    )
) else (
    echo ✅ Database güncel
)

cd ..\..

echo.
echo ==========================================
echo API ve Web başlatılıyor...
echo ==========================================
echo.
echo İki ayrı pencere açılacak:
echo   1. API  → http://localhost:5074
echo   2. Web  → http://localhost:5248
echo.
echo HOT RELOAD AKTİF!
echo Kod değiştirdiğinizde otomatik yeniden yüklenecek.
echo.

REM API'yi başlat
start "ETicaret API (Dev Mode)" cmd /k "cd /d %~dp0src\ETicaret.API && echo ========================================== && echo API Başlatılıyor (Hot Reload Aktif) && echo ========================================== && echo. && dotnet watch run --launch-profile http"

REM 10 saniye bekle
echo API başlatıldı. Web için 10 saniye bekleniyor...
timeout /t 10 /nobreak > nul

REM Web'i başlat
start "ETicaret Web (Dev Mode)" cmd /k "cd /d %~dp0src\ETicaret.Web && echo ========================================== && echo Web Başlatılıyor (Hot Reload Aktif) && echo ========================================== && echo. && dotnet watch run --launch-profile http"

echo.
echo ==========================================
echo ✅ Development ortamı hazır!
echo ==========================================
echo.
echo Servisler:
echo   🔌 API:     http://localhost:5074
echo   📚 Swagger: http://localhost:5074/swagger
echo   🌐 Web:     http://localhost:5248
echo   🐰 RabbitMQ UI: http://localhost:15672 (guest/guest)
echo.
echo Admin Hesabı:
echo   Email: admin@eticaret.com
echo   Şifre: Admin123!
echo.
echo HOT RELOAD:
echo   - Kod değiştirdiğinizde otomatik yenilenir
echo   - CSS değişiklikleri anında görünür
echo   - Yeni sayfalar hemen çalışır
echo.
echo Durdurmak için:
echo   - Her iki terminal penceresini kapatın
echo   - Docker servisleri: docker-compose -f docker-compose.dev.yml down
echo.
echo ==========================================
echo.

timeout /t 5 > nul
start http://localhost:5248

pause
