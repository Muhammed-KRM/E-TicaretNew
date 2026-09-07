@echo off
echo ==========================================
echo ETicaret API Başlatılıyor...
echo ==========================================
echo.
echo API Adresi: http://localhost:5074
echo Swagger: http://localhost:5074/swagger
echo.
echo Durdurmak için Ctrl+C yapabilirsiniz.
echo ==========================================
echo.

cd /d "%~dp0src\ETicaret.API"
dotnet run --launch-profile http

pause
