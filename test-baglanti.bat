@echo off
echo ==========================================
echo PostgreSQL Bağlantı Testi
echo ==========================================
echo.
echo PostgreSQL bilgileri:
echo   Host: localhost
echo   Port: 5432
echo   Database: ETicaret
echo   Username: ETicaret_user
echo.
echo Test ediliyor...
echo.

REM PostgreSQL'e bağlan ve database'i kontrol et
psql -h localhost -p 5432 -U ETicaret_user -d ETicaret -c "\dt" 2>nul

if %errorlevel% equ 0 (
    echo.
    echo ✅ PostgreSQL bağlantısı başarılı!
    echo ✅ Database ETicaret mevcut.
    echo.
) else (
    echo.
    echo ❌ PostgreSQL bağlantısı başarısız!
    echo.
    echo Olası sorunlar:
    echo   1. PostgreSQL çalışmıyor olabilir
    echo   2. Database henüz oluşturulmamış
    echo   3. Kullanıcı adı/şifre hatalı
    echo.
    echo Çözüm:
    echo   1. PostgreSQL'in çalıştığından emin olun
    echo   2. Database'i manuel oluşturun:
    echo      psql -U postgres -c "CREATE DATABASE \"ETicaret\";"
    echo      psql -U postgres -c "CREATE USER ETicaret_user WITH PASSWORD 'OzelDers_Dev_2024!';"
    echo      psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE \"ETicaret\" TO ETicaret_user;"
    echo.
)

pause
