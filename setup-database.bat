@echo off
echo ==========================================
echo ETicaret Database Kurulumu
echo ==========================================
echo.
echo Bu script PostgreSQL database'ini oluşturacak.
echo.
echo ÖNEMLI: PostgreSQL'in kurulu ve çalışıyor olması gerekir!
echo.
pause
echo.
echo Database oluşturuluyor...
echo.

REM PostgreSQL süper kullanıcısı ile bağlan ve database oluştur
psql -U postgres -c "CREATE DATABASE \"ETicaret\";" 2>nul
if %errorlevel% equ 0 (
    echo ✅ Database oluşturuldu: ETicaret
) else (
    echo ⚠️ Database zaten mevcut veya oluşturulamadı
)

echo.
echo Kullanıcı oluşturuluyor...
echo.

REM Kullanıcı oluştur
psql -U postgres -c "CREATE USER ETicaret_user WITH PASSWORD 'OzelDers_Dev_2024!';" 2>nul
if %errorlevel% equ 0 (
    echo ✅ Kullanıcı oluşturuldu: ETicaret_user
) else (
    echo ⚠️ Kullanıcı zaten mevcut veya oluşturulamadı
)

echo.
echo Yetkiler veriliyor...
echo.

REM Database yetkilerini ver
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE \"ETicaret\" TO ETicaret_user;"
psql -U postgres -d ETicaret -c "GRANT ALL ON SCHEMA public TO ETicaret_user;"
psql -U postgres -d ETicaret -c "GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ETicaret_user;"
psql -U postgres -d ETicaret -c "GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO ETicaret_user;"

echo.
echo ✅ Database kurulumu tamamlandı!
echo.
echo Detaylar:
echo   Database: ETicaret
echo   Kullanıcı: ETicaret_user
echo   Şifre: OzelDers_Dev_2024!
echo   Host: localhost
echo   Port: 5432
echo.
echo Şimdi migration'ları çalıştırabilirsiniz:
echo   cd src\ETicaret.API
echo   dotnet ef database update
echo.

pause
