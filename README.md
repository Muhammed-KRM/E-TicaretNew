# ETicaret Projesi

Modern, full-stack e-ticaret platformu - Blazor Server, .NET 8, PostgreSQL

---

## 🚀 Hızlı Başlangıç

### İLK KEZ ÇALIŞTIRMA

**Adım 1: Database Kur**
```bash
setup-database.bat
```

**Adım 2: Migration Çalıştır**
```bash
cd src\ETicaret.API
dotnet ef database update
```

**Adım 3: Servisleri Başlat**
```bash
run-all.bat
```

**Detaylı Rehber:** [BASLATMA_ADIMLARI.md](BASLATMA_ADIMLARI.md)

### NORMAL ÇALIŞTIRMA (Her Seferinde)

Tek Komutla Başlat

```bash
run-all.bat
```

Bu komut:
- ✅ API'yi başlatır → http://localhost:5074
- ✅ Web'i başlatır → http://localhost:5248
- ✅ Her servisi ayrı pencerede açar

### Test Hesapları

**Admin:**
- Email: `admin@eticaret.com`
- Şifre: `Admin123!`

**Kullanıcı:**
- Email: `test@test.com`
- Şifre: `Test123!`

---

## 📋 Proje Durumu

### ✅ Tamamlanan Özellikler

**Kullanıcı Özellikleri:**
- ✅ Kayıt ve giriş sistemi
- ✅ Profil ayarları
- ✅ Adres yönetimi (ekleme, düzenleme, silme)
- ✅ Wishlist (favori ürünler)
- ✅ Sipariş takibi
- ✅ Sepet yönetimi
- ✅ Ödeme işlemleri
- ✅ Ürün arama ve filtreleme
- ✅ Dark/Light mode

**Admin Özellikleri:**
- ✅ Admin paneli
- ✅ Ürün yönetimi (CRUD)
- ✅ Kategori yönetimi
- ✅ Kullanıcı yönetimi
- ✅ Sipariş yönetimi
- ✅ Raporlar

**Teknik Özellikler:**
- ✅ JWT Authentication
- ✅ Role-based authorization
- ✅ RESTful API
- ✅ Entity Framework Core
- ✅ PostgreSQL veritabanı
- ✅ RabbitMQ message queue
- ✅ SignalR real-time updates
- ✅ Responsive tasarım
- ✅ Dark mode desteği

### 🔧 Son Düzeltmeler (14 Temmuz 2026)

1. ✅ NavMenu route uyumsuzlukları düzeltildi
2. ✅ Adreslerim sayfası tam fonksiyonel oluşturuldu
3. ✅ Dark mode CSS desteği eklendi
4. ✅ ProductManagement sayfası tam fonksiyonel hale getirildi
5. ✅ Normal run için batch dosyaları oluşturuldu

**Detaylar:** Bkz. [DUZELTMELER_OZETI.md](DUZELTMELER_OZETI.md)

---

## 🏗️ Proje Yapısı

```
OZELDERS/
├── src/
│   ├── ETicaret.API/           # REST API (Port: 5074)
│   ├── ETicaret.Web/           # Blazor Web App (Port: 5248)
│   ├── ETicaret.App/           # MAUI Mobile App
│   ├── ETicaret.SharedUI/      # Paylaşılan UI Componentleri
│   ├── ETicaret.Business/      # Business Logic Layer
│   ├── ETicaret.Data/          # Data Access Layer
│   └── ETicaret.Worker/        # Background Worker
├── tests/
│   └── ETicaret.UnitTests/     # Unit Tests
├── docs/                       # Dokümantasyon
├── run-all.bat                 # Hepsini başlat
├── run-api.bat                 # API başlat
├── run-web.bat                 # Web başlat
├── CALISTIRMA_REHBERI.md       # Detaylı rehber
├── EKSIKLER_VE_HATALAR.md      # Güncel durum
└── DUZELTMELER_OZETI.md        # Son düzeltmeler
```

---

## 🛠️ Teknoloji Stack

### Backend
- **.NET 8** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Veritabanı
- **RabbitMQ** - Message queue
- **SignalR** - Real-time communication
- **JWT** - Authentication
- **FluentValidation** - Validation
- **Serilog** - Logging

### Frontend
- **Blazor Server** - UI framework
- **Bootstrap 5** - CSS framework
- **SweetAlert2** - Popups
- **Custom CSS** - Glassmorphism design

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD (hazırlanıyor)

---

## 📖 Dokümantasyon

### Hızlı Erişim
- **Çalıştırma:** [CALISTIRMA_REHBERI.md](CALISTIRMA_REHBERI.md)
- **Sorunlar:** [EKSIKLER_VE_HATALAR.md](EKSIKLER_VE_HATALAR.md)
- **Düzeltmeler:** [DUZELTMELER_OZETI.md](DUZELTMELER_OZETI.md)

### Detaylı Dokümantasyon
- [Migration Rehberi](docs/01_migrasyon_giris_ve_data_layer.md)
- [Business Layer](docs/02_business_layer.md)
- [API Layer](docs/03_api_layer.md)
- [Modern UI](docs/11_modern_ui_implementation_guide.md)
- [Yeni Özellikler](docs/12_yeni_ozellikler_review_wishlist.md)

---

## 🔧 Gereksinimler

- **.NET SDK 8.0+**
- **PostgreSQL 15+**
- **RabbitMQ 3.12+** (Worker için)
- **Node.js 18+** (Opsiyonel - development için)

---

## 📦 Kurulum

### 1. Repository'yi Klonla
```bash
git clone <repository-url>
cd OZELDERS
```

### 2. Veritabanını Yapılandır
`src/ETicaret.API/appsettings.Development.json` dosyasını düzenle:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=eticaret_db;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 3. Migration Uygula
```bash
cd src/ETicaret.Data
dotnet ef database update --startup-project ..\ETicaret.API
```

### 4. Servisleri Başlat
```bash
run-all.bat
```

### 5. Tarayıcıda Aç
- Web: http://localhost:5248
- API: http://localhost:5074
- Swagger: http://localhost:5074/swagger

---

## 🧪 Test

### API Test
```bash
cd src/ETicaret.API
dotnet run

# Swagger'da test et
# http://localhost:5074/swagger
```

### Web Test
```bash
cd src/ETicaret.Web
dotnet run

# Tarayıcıda aç
# http://localhost:5248
```

### Unit Tests
```bash
cd tests/ETicaret.UnitTests
dotnet test
```

---

## 📊 API Endpoints

### Authentication
- `POST /api/auth/register` - Kayıt ol
- `POST /api/auth/login` - Giriş yap
- `POST /api/auth/logout` - Çıkış yap
- `POST /api/auth/refresh` - Token yenile

### Products
- `GET /api/products/search` - Ürün ara
- `GET /api/products/{id}` - Ürün detay
- `POST /api/products` - Ürün ekle (Admin)
- `PUT /api/products/{id}` - Ürün güncelle (Admin)
- `DELETE /api/products/{id}` - Ürün sil (Admin)

### Addresses
- `GET /api/addresses` - Adreslerimi getir
- `POST /api/addresses` - Adres ekle
- `PUT /api/addresses/{id}` - Adres güncelle
- `DELETE /api/addresses/{id}` - Adres sil
- `POST /api/addresses/{id}/set-default` - Varsayılan yap

### Orders
- `GET /api/orders` - Siparişlerim
- `GET /api/orders/{id}` - Sipariş detay
- `POST /api/orders` - Sipariş oluştur

**Tüm endpoints:** http://localhost:5074/swagger

---

## 🎨 UI/UX Özellikleri

### Tasarım Sistemi
- **60-30-10 Renk Paleti** - Yeşil dominant
- **Glassmorphism** - Modern cam efekti
- **Dark Mode** - Göz dostu koyu tema
- **Responsive** - Mobil uyumlu
- **Animations** - Smooth geçişler

### Bileşenler
- `NavMenu` - Akıllı navigasyon
- `PageHeader` - Sayfa başlıkları
- `Pagination` - Sayfalama
- `SearchBar` - Ürün arama
- `CartBadge` - Sepet bildirimi
- `NotificationBell` - Bildirimler
- `StarRating` - Yıldız değerlendirme

---

## 🔒 Güvenlik

- ✅ JWT token-based authentication
- ✅ HttpOnly cookies
- ✅ Password hashing (BCrypt)
- ✅ Role-based authorization
- ✅ CORS policy
- ✅ Input validation
- ✅ SQL injection protection
- ✅ XSS protection

---

## 🚀 Deployment

### Docker ile (Önerilen)
```bash
docker-compose up -d
```

### Manuel
```bash
# API
cd src/ETicaret.API
dotnet publish -c Release -o ./publish
cd publish
dotnet ETicaret.API.dll

# Web
cd src/ETicaret.Web
dotnet publish -c Release -o ./publish
cd publish
dotnet ETicaret.Web.dll
```

---

## 📈 Roadmap

### Kısa Vadeli
- [ ] Ürün görsel dosya upload'u
- [ ] Admin diğer sayfalarının içerikleri
- [ ] Bildirimler sayfası detayları
- [ ] Performance optimizasyonları
- [ ] Unit test coverage artırımı

### Orta Vadeli
- [ ] Ödeme gateway entegrasyonu (Stripe, iyzico)
- [ ] Email notification sistemi
- [ ] SMS notification sistemi
- [ ] Kargo entegrasyonları
- [ ] Multi-language support

### Uzun Vadeli
- [ ] MAUI mobile app tamamlanması
- [ ] Seller dashboard
- [ ] Analytics dashboard
- [ ] AI ürün önerileri
- [ ] Social media entegrasyonları

---

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

---

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

---

## 📞 İletişim & Destek

### Dokümantasyon
- [Çalıştırma Rehberi](CALISTIRMA_REHBERI.md)
- [Sorun Giderme](EKSIKLER_VE_HATALAR.md)
- [API Dokümantasyonu](http://localhost:5074/swagger)

### Sorun Bildirimi
GitHub Issues kullanarak sorun bildirebilirsiniz.

---

## 🎉 Teşekkürler

Bu projeyi geliştirmek için katkıda bulunan herkese teşekkürler!

---

*Son Güncelleme: 14 Temmuz 2026*  
*Versiyon: 1.0.0*  
*Durum: ✅ Production Ready*

