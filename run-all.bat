@echo off
echo ==========================================
echo ETicaret OZELDERS - Tüm Servisler Başlatılıyor...
echo ==========================================
echo.
echo PROJE KONUMU: D:\ETicaret\OZELDERS
echo.

REM PostgreSQL kontrolü
echo PostgreSQL kontrolü yapılıyor...
netstat -ano | findstr :5432 > nul
if %errorlevel% neq 0 (
    echo.
    echo ❌ HATA: PostgreSQL çalışmıyor!
    echo.
    echo PostgreSQL kurulu değil veya başlatılmamış.
    echo.
    echo Çözüm için:
    echo   1. POSTGRESQL_KURULUM.md dosyasını okuyun
    echo   2. PostgreSQL kurun ve başlatın
    echo   3. setup-database.bat çalıştırın
    echo.
    pause
    exit /b 1
)

echo ✅ PostgreSQL çalışıyor (Port 5432)
echo.

echo Bu script aşağıdaki servisleri başlatacak:
echo   1. API     - http://localhost:5074
echo   2. Web App - http://localhost:5248
echo.
echo Her servis ayrı pencerede açılacaktır.
echo Servisleri durdurmak için her pencereyi kapatın.
echo ==========================================
echo.
echo ÖNEMLI: İlk çalıştırmadan önce:
echo   1. PostgreSQL çalışıyor olmalı ✅
echo   2. Database kurulmalı (setup-database.bat)
echo   3. Migration'lar uygulanmalı (dotnet ef database update)
echo.
echo Detaylı bilgi için: BASLATMA_ADIMLARI.md
echo.
pause
echo.

REM API'yi başlat
start "ETicaret API" cmd /k "cd /d %~dp0src\ETicaret.API && echo API Başlatılıyor... && dotnet run --launch-profile http"

REM 5 saniye bekle (API'nin başlaması için)
echo API başlatıldı. Web uygulaması için 5 saniye bekleniyor...
timeout /t 5 /nobreak > nul

REM Web uygulamasını başlat
start "ETicaret Web" cmd /k "cd /d %~dp0src\ETicaret.Web && echo Web Uygulaması Başlatılıyor... && dotnet run --launch-profile http"

echo.
echo ==========================================
echo Tüm servisler başlatıldı!
echo ==========================================
echo.
echo   API:     http://localhost:5074
echo   Swagger: http://localhost:5074/swagger
echo   Web App: http://localhost:5248
echo.
echo Tarayıcınızda http://localhost:5248 adresini açın.
echo.
echo Servisleri durdurmak için açılan terminal pencerelerini kapatın.
echo ==========================================

pause
