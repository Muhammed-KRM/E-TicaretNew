# ETicaret Projesi - İlk Çalıştırma Adımları

**Önemli:** Bu adımları sadece ilk kez çalıştırırken yapmanız gerekir.

---

## 📋 Ön Kontroller

### 1. PostgreSQL Kurulu mu?
```bash
psql --version
```
Eğer kurulu değilse: https://www.postgresql.org/download/

### 2. .NET 8 Kurulu mu?
```bash
dotnet --version
```
Eğer kurulu değilse: https://dotnet.microsoft.com/download

---

## 🚀 İlk Kurulum (Sadece Bir Kez)

### Adım 1: Database Oluştur

**Otomatik Yöntem (Önerilen):**
```bash
setup-database.bat
```

**Manuel Yöntem:**
```bash
# PostgreSQL'e bağlan
psql -U postgres

# Komutları sırayla çalıştır:
CREATE DATABASE "ETicaret";
CREATE USER ETicaret_user WITH PASSWORD 'OzelDers_Dev_2024!';
GRANT ALL PRIVILEGES ON DATABASE "ETicaret" TO ETicaret_user;
\q
```

### Adım 2: Migration Çalıştır

```bash
cd src\ETicaret.API
dotnet ef database update
```

Eğer migration dosyaları yoksa:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Adım 3: Bağlantıyı Test Et

```bash
test-baglanti.bat
```

✅ Eğer bağlantı başarılıysa, devam edebilirsiniz!

---

## 🎯 Normal Çalıştırma (Her Seferinde)

### Tüm Servisleri Başlat

```bash
run-all.bat
```

Bu komut:
1. API'yi başlatır → http://localhost:5074
2. Web'i başlatır → http://localhost:5248

### Ayrı Ayrı Başlat

**Sadece API:**
```bash
run-api.bat
```

**Sadece Web:**
```bash
run-web.bat
```

---

## 🔑 Test Hesapları

### Admin Hesabı
- Email: `admin@eticaret.com`
- Şifre: `Admin123!`

### Test Kullanıcı
- Email: `test@test.com`
- Şifre: `Test123!`

**Not:** Bu hesaplar otomatik seed data ile oluşturulur.

---

## 🧪 Test Adımları

### 1. API Testi
- Tarayıcıda aç: http://localhost:5074/swagger
- ✅ Swagger UI açılmalı
- ✅ Endpoint'ler listelenmeli

### 2. Web Testi
- Tarayıcıda aç: http://localhost:5248
- ✅ Ana sayfa açılmalı
- ✅ Giriş yapabilmeli

### 3. Giriş Testi
- Email: admin@eticaret.com
- Şifre: Admin123!
- ✅ Admin paneline erişebilmeli

### 4. Özellik Testleri
- ✅ Dark mode toggle (🌙 butonu)
- ✅ Profil Ayarları → `/panel/ayarlar`
- ✅ Adreslerim → `/panel/adreslerim`
- ✅ Admin Paneli → `/admin`
- ✅ Ürün Yönetimi → `/admin/urun-ekle`

---

## ❌ Sorun Giderme

### Hata: "Database does not exist"

**Çözüm:**
```bash
setup-database.bat
```

### Hata: "Could not connect to database"

**Kontroller:**
1. PostgreSQL çalışıyor mu?
   ```bash
   # Windows'ta servis kontrol et
   sc query postgresql-x64-15
   ```

2. Port kullanımda mı?
   ```bash
   netstat -ano | findstr :5432
   ```

3. Bağlantı bilgileri doğru mu?
   - Dosya: `src\ETicaret.API\appsettings.Development.json`
   - Host: localhost
   - Port: 5432
   - Database: ETicaret
   - Username: ETicaret_user
   - Password: OzelDers_Dev_2024!

### Hata: "Migration pending"

**Çözüm:**
```bash
cd src\ETicaret.API
dotnet ef database update
```

### Hata: "Port already in use"

**API (5074) veya Web (5248) portu kullanımda:**

**Kontrol:**
```bash
netstat -ano | findstr :5074
netstat -ano | findstr :5248
```

**Çözüm 1:** Process'i kapat
```bash
taskkill /PID [PID_NUMARASI] /F
```

**Çözüm 2:** Port değiştir
- API: `src\ETicaret.API\Properties\launchSettings.json`
- Web: `src\ETicaret.Web\Properties\launchSettings.json`

### Hata: "Could not load file or assembly"

**Çözüm:**
```bash
# Tüm projede restore yap
dotnet restore

# Clean build
dotnet clean
dotnet build
```

---

## 📁 Önemli Dosyalar

### Yapılandırma
- `src\ETicaret.API\appsettings.Development.json` - API ayarları
- `src\ETicaret.Web\appsettings.json` - Web ayarları
- `src\ETicaret.API\Properties\launchSettings.json` - Port ayarları

### Database
- `src\ETicaret.Data\Context\ApplicationDbContext.cs` - Database context
- `src\ETicaret.Data\Migrations\` - Migration dosyaları

### Başlatma
- `run-all.bat` - Tüm servisleri başlat
- `run-api.bat` - API başlat
- `run-web.bat` - Web başlat
- `setup-database.bat` - Database kur
- `test-baglanti.bat` - Bağlantıyı test et

---

## 🎉 Başarılı Çalıştırma Kriterleri

✅ PostgreSQL çalışıyor  
✅ Database oluşturuldu (ETicaret)  
✅ Migration'lar uygulandı  
✅ API çalışıyor (http://localhost:5074)  
✅ Swagger açılıyor (http://localhost:5074/swagger)  
✅ Web çalışıyor (http://localhost:5248)  
✅ Admin girişi yapılabiliyor  
✅ Tüm sayfalar yükleniyor  
✅ Dark mode çalışıyor  

---

## 🆘 Yardım

Hala sorun yaşıyorsanız:

1. Terminal'deki hata mesajını tam olarak okuyun
2. `CALISTIRMA_REHBERI.md` dosyasına bakın
3. `EKSIKLER_VE_HATALAR.md` dosyasını kontrol edin
4. Browser console'u açın (F12) ve hataları kontrol edin

---

## 📞 Hızlı Komutlar

```bash
# Database kur
setup-database.bat

# Bağlantıyı test et
test-baglanti.bat

# Migration çalıştır
cd src\ETicaret.API
dotnet ef database update

# Hepsini başlat
run-all.bat

# API başlat
run-api.bat

# Web başlat  
run-web.bat
```

---

**Not:** Bu dokümanda `D:\ETicaret\OZELDERS` klasöründeki asıl projemiz anlatılmaktadır.  
`D:\ETicaret\docs\E-ticaret - Kopya` klasörü eski örnek projedir, ona dokunmayın!

