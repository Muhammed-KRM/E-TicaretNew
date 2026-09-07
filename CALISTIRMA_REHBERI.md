# ETicaret Projesi - Çalıştırma Rehberi

## 🚀 Hızlı Başlangıç

### Otomatik Başlatma (Önerilen)

Tüm servisleri tek seferde başlatmak için:

```bash
run-all.bat
```

Bu dosya:
- ✅ API'yi başlatır (http://localhost:5074)
- ✅ Web uygulamasını başlatır (http://localhost:5248)
- ✅ Her servisi ayrı terminal penceresinde açar
- ✅ Otomatik olarak gerekli bekleme sürelerini ekler

### Manuel Başlatma

Servisleri ayrı ayrı başlatmak isterseniz:

#### 1. API'yi Başlatın (Önce bu!)
```bash
run-api.bat
```
veya
```bash
cd src\ETicaret.API
dotnet run --launch-profile http
```

**API Adresleri:**
- API: http://localhost:5074
- Swagger: http://localhost:5074/swagger

#### 2. Web Uygulamasını Başlatın
```bash
run-web.bat
```
veya
```bash
cd src\ETicaret.Web
dotnet run --launch-profile http
```

**Web Adresi:**
- Web App: http://localhost:5248

---

## 📋 Ön Gereksinimler

### 1. .NET SDK 8.0
```bash
dotnet --version
```
En az .NET 8.0 yüklü olmalı.

### 2. PostgreSQL
PostgreSQL veritabanı çalışıyor olmalı.

**Bağlantı Bilgileri** (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=eticaret_db;Username=postgres;Password=your_password"
  }
}
```

### 3. RabbitMQ (Opsiyonel - Worker için)
Worker servisini çalıştıracaksanız RabbitMQ gerekli.

---

## 🔧 İlk Kurulum

### 1. Veritabanı Oluşturma

#### Otomatik Migration (Önerilen)
Uygulama ilk çalıştığında otomatik olarak veritabanını oluşturur.

#### Manuel Migration
```bash
cd src\ETicaret.Data
dotnet ef database update --startup-project ..\ETicaret.API
```

### 2. Seed Data
İlk çalıştırmada otomatik olarak:
- ✅ Admin kullanıcısı oluşturulur
- ✅ Test kullanıcısı oluşturulur
- ✅ Örnek kategoriler eklenir
- ✅ Örnek ürünler eklenir

**Admin Hesabı:**
- Email: `admin@eticaret.com`
- Şifre: `Admin123!`

**Test Hesabı:**
- Email: `test@test.com`
- Şifre: `Test123!`

---

## 🧪 Test Senaryoları

### 1. API Testi
```bash
# API çalışıyor mu?
curl http://localhost:5074/health

# Swagger'ı aç
# http://localhost:5074/swagger
```

### 2. Web Uygulaması Testi
1. Tarayıcıda `http://localhost:5248` aç
2. Giriş yapın: `admin@eticaret.com` / `Admin123!`
3. Test et:
   - ✅ Ana sayfa yükleniyor mu?
   - ✅ Profil Ayarları (`/panel/ayarlar`)
   - ✅ Adreslerim (`/panel/adreslerim`)
   - ✅ Admin Paneli (`/admin` veya `/admin/dashboard`)
   - ✅ Ürün Yönetimi (`/admin/urun-ekle`)
   - ✅ Dark Mode toggle (🌙 butonu)

### 3. Ürün Yönetimi Testi
1. Admin girişi yap
2. "Ürün Ekle" butonuna tıkla
3. Form doldur:
   - Ürün adı
   - Açıklama (min 20 karakter)
   - Kategori seç
   - Fiyat gir
   - Stok miktarı
   - Görsel URL ekle
4. Kaydet
5. Ürünün listede göründüğünü kontrol et

### 4. Adres Yönetimi Testi
1. "Adreslerim" sayfasına git
2. "Yeni Adres Ekle" tıkla
3. Form doldur
4. Kaydet
5. Düzenleme/silme işlemlerini test et

---

## 🐛 Sorun Giderme

### API Çalışmıyor

**Hata:** `Unable to connect to database`
```bash
# PostgreSQL çalışıyor mu kontrol et
# Connection string'i kontrol et
# appsettings.Development.json dosyasını düzenle
```

**Hata:** `Port 5074 already in use`
```bash
# Portu değiştir (launchSettings.json)
# veya çalışan uygulamayı durdur
```

### Web Uygulaması Çalışmıyor

**Hata:** `API call failed`
```bash
# 1. API'nin çalıştığından emin ol (http://localhost:5074)
# 2. Web appsettings.json'da API URL'sini kontrol et
```

**Hata:** `404 Not Found` (Route hatası)
```bash
# NavMenu route'ları kontrol et
# EKSIKLER_VE_HATALAR.md dosyasına bak
```

### Dark Mode Çalışmıyor

**Sorun:** Buton tıklanıyor ama tema değişmiyor
```bash
# 1. Browser console'u aç (F12)
# 2. JavaScript hatası var mı kontrol et
# 3. interop.js yüklendi mi kontrol et
# 4. variables.css'te dark mode stilleri var mı kontrol et
```

**Çözüm:** Bu sorun düzeltildi! Dark mode CSS desteği eklendi.

### Build Hataları

```bash
# NuGet paketlerini geri yükle
dotnet restore

# Temiz build
dotnet clean
dotnet build

# Cache temizle
dotnet nuget locals all --clear
```

---

## 📊 Port Kullanımı

| Servis | Port | URL |
|--------|------|-----|
| API | 5074 | http://localhost:5074 |
| Swagger | 5074 | http://localhost:5074/swagger |
| Web App | 5248 | http://localhost:5248 |
| PostgreSQL | 5432 | localhost:5432 |
| RabbitMQ | 5672 | localhost:5672 |
| RabbitMQ UI | 15672 | http://localhost:15672 |

---

## 🔒 Güvenlik

### JWT Token
- Token ömrü: 1 gün
- Refresh token: Destekleniyor
- Cookie-based: Evet (HttpOnly)

### Roller
- **Admin:** Tüm yetkilere sahip
- **User:** Normal kullanıcı yetkileri
- **Seller:** Satıcı yetkileri (gelecekte)

---

## 📁 Proje Yapısı

```
OZELDERS/
├── src/
│   ├── ETicaret.API/           # REST API
│   ├── ETicaret.Web/           # Blazor Server Web App
│   ├── ETicaret.App/           # MAUI Mobile App
│   ├── ETicaret.SharedUI/      # Paylaşılan Blazor Components
│   ├── ETicaret.Business/      # Business Logic
│   ├── ETicaret.Data/          # Data Access Layer
│   └── ETicaret.Worker/        # Background Jobs
├── tests/
│   └── ETicaret.UnitTests/     # Unit Tests
├── run-all.bat                 # Tüm servisleri başlat
├── run-api.bat                 # Sadece API başlat
├── run-web.bat                 # Sadece Web başlat
├── baslat.bat                  # Docker ile başlat
└── durdur.bat                  # Docker ile durdur
```

---

## 🎯 Sonraki Adımlar

### Tamamlanan ✅
- [x] NavMenu route düzeltmeleri
- [x] Adreslerim sayfası
- [x] Dark mode CSS desteği
- [x] ProductManagement sayfası (CRUD)
- [x] Normal run için batch dosyaları

### Yapılacaklar 📝
- [ ] Ürün görsel upload (dosya yükleme)
- [ ] Admin diğer sayfaları (Siparişler, Kullanıcılar, Raporlar)
- [ ] Bildirimler sayfası içeriği
- [ ] Responsive tasarım iyileştirmeleri
- [ ] Unit testlerin tamamlanması
- [ ] Production deployment

---

## 📞 Yardım

Sorun yaşarsanız:
1. **EKSIKLER_VE_HATALAR.md** dosyasına bakın
2. **DUZELTMELER_OZETI.md** dosyasını okuyun
3. Browser console'u kontrol edin (F12)
4. API loglarını kontrol edin
5. Database bağlantısını test edin

---

## 🎉 Başarıyla Çalıştırma

Tüm adımları tamamladıysanız:
1. ✅ API çalışıyor: http://localhost:5074/swagger
2. ✅ Web çalışıyor: http://localhost:5248
3. ✅ Admin giriş yapabiliyorsunuz
4. ✅ Tüm sayfalar çalışıyor
5. ✅ Dark mode çalışıyor
6. ✅ CRUD işlemleri çalışıyor

**Tebrikler! 🚀 Proje başarıyla çalışıyor.**

