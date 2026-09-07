@echo off
echo ==========================================
echo Docker Yeniden Build ve Başlatma
echo ==========================================
echo.
echo Bu işlem:
echo   1. Mevcut container'ları durdurup silecek
echo   2. Image'leri yeniden build edecek
echo   3. Son kodla yeniden başlatacak
echo.
echo UYARI: İşlem 5-10 dakika sürebilir!
echo.
pause
echo.

echo Container'lar durduruluyor ve siliniyor...
docker-compose down

echo.
echo Image'ler yeniden build ediliyor...
docker-compose build --no-cache web api

echo.
echo Container'lar başlatılıyor...
docker-compose up -d

echo.
echo ==========================================
echo ✅ Yeniden build tamamlandı!
echo ==========================================
echo.
echo Container'ların hazır olması için 30 saniye bekleniyor...
timeout /t 30 /nobreak > nul

echo.
echo Tarayıcıda açılıyor: http://localhost:8080
start http://localhost:8080

echo.
echo Logları görmek için:
echo   docker-compose logs -f web
echo   docker-compose logs -f api
echo.

pause
