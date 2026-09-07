@echo off
echo ==========================================
echo ETicaret Web Uygulaması Başlatılıyor...
echo ==========================================
echo.
echo Web Adresi: http://localhost:5248
echo.
echo ÖNEMLI: API'nin çalıştığından emin olun!
echo API çalışmıyorsa run-api.bat dosyasını çalıştırın.
echo.
echo Durdurmak için Ctrl+C yapabilirsiniz.
echo ==========================================
echo.

cd /d "%~dp0src\ETicaret.Web"
dotnet run --launch-profile http

pause
