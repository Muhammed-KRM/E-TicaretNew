# Docker ile ETicaret Başlatma Rehberi

## 🐳 Docker ile Başlatma (En Kolay Yöntem)

PostgreSQL kurmak istemiyorsanız, Docker ile her şey otomatik olarak kurulur!

---

## ⚡ Hızlı Başlatma

### 1. Docker Desktop'ı Başlat

Docker Desktop uygulamasını açın ve tamamen başlamasını bekleyin (30-60 saniye).

### 2. Kontrol Et

```bash
docker-kontrol.bat
```

✅ "Docker Desktop çalışıyor!" mesajını görmelisiniz.

### 3. Başlat

```bash
baslat.bat
```

Bu komut:
- ✅ PostgreSQL container'ı başlatır
- ✅ Redis container'ı başlatır  
- ✅ RabbitMQ container'ı başlatır
- ✅ API container'ı başlatır
- ✅ Web container'ı başlatır
- ✅ Nginx container'ı başlatır
- ✅ Veritabanı migration'larını otomatik çalıştırır
- ✅ Seed data'yı yükler

### 4. Erişim

**İlk seferde 2-5 dakika sürebilir (image'ler indirilecek)**

Hazır olduğunda:
- 🌐 **Web:** http://localhost:8080
- 🔌 **API:** http://localhost:5001
- 📊 **Swagger:** http://localhost:5001/swagger
- 🐰 **RabbitMQ UI:** http://localhost:15672 (guest/guest)

---

## 🎯 Admin Hesabı

Otomatik oluşturulan admin hesabı:

```
Email: admin@eticaret.com
Şifre: Admin123!
```

---

## 🛠️ Docker Komutları

### Container Durumunu Görme

```bash
docker-compose ps
```

### Logları Görme

```bash
# Tüm servisler
docker-compose logs -f

# Sadece API
docker-compose logs -f api

# Sadece Web
docker-compose logs -f web
```

### Yeniden Başlatma

```bash
# Tek komut
docker-compose restart

# Veya durdur/başlat
durdur.bat
baslat.bat
```

### Temizlik (Yeniden Build)

```bash
# Container'ları durdur ve sil
docker-compose down

# Volume'leri de sil (VERİTABANI SİLİNİR!)
docker-compose down -v

# Yeniden başlat
baslat.bat
```

---

## 🔧 Port Yapılandırması

Docker container'ları bu portları kullanır:

| Servis | Host Port | Container Port |
|--------|-----------|----------------|
| Web | 8080 | 8080 |
| API | 5001 | 8080 |
| Nginx | 80, 443 | 80, 443 |
| PostgreSQL | 5432 | 5432 |
| Redis | 6379 | 6379 |
| RabbitMQ | 5672, 15672 | 5672, 15672 |
| Elasticsearch | 9200 | 9200 |

---

## ❓ Sorun Giderme

### Hata: "Docker Desktop çalışmıyor"

**Çözüm:**
1. Docker Desktop uygulamasını başlatın
2. System tray'de Docker simgesinin yeşil olmasını bekleyin
3. `docker-kontrol.bat` çalıştırın
4. ✅ görürseniz `baslat.bat` çalıştırın

### Hata: "port already in use"

**Çözüm 1:** Başka bir uygulama portu kullanıyor

```bash
# Port 8080 kullanımda
netstat -ano | findstr :8080

# Process'i kapat
taskkill /PID [PID] /F
```

**Çözüm 2:** `docker-compose.yml` dosyasında portları değiştir

```yaml
services:
  web:
    ports: ["8081:8080"]  # 8081 kullan
```

### Hata: "Container health check failed"

**Çözüm:** Container loglarına bak

```bash
docker-compose logs api
docker-compose logs web
```

### Hata: "Database migration failed"

**Çözüm:** Container'ı yeniden başlat

```bash
docker-compose restart api
docker-compose logs -f api
```

### Web açılmıyor / 502 Bad Gateway

**Neden:** Container'lar henüz hazır değil

**Çözüm:** Biraz bekleyin (30 saniye), sonra yenileyin

```bash
# Container durumlarını kontrol et
docker-compose ps

# Tüm container'lar "Up (healthy)" olmalı
```

---

## 🗂️ Veri Yönetimi

### Volume'ler

Docker, veritabanı ve diğer verileri volume'lerde saklar:

```bash
# Volume'leri listele
docker volume ls

# OZELDERS ile başlayan volume'ler bizim
```

### Yedekleme

```bash
# PostgreSQL veritabanını yedekle
docker-compose exec postgres pg_dump -U ETicaret_user ETicaret > backup.sql

# Geri yükle
docker-compose exec -T postgres psql -U ETicaret_user ETicaret < backup.sql
```

### Temiz Başlangıç

```bash
# Tüm container'ları ve volume'leri sil
docker-compose down -v

# Yeniden başlat
baslat.bat
```

⚠️ **DİKKAT:** `-v` parametresi tüm verileri siler!

---

## 🚀 Production Deployment

Docker Compose dosyası production-ready:

- ✅ Multi-stage builds (optimize image'ler)
- ✅ Health checks
- ✅ Resource limits
- ✅ Nginx reverse proxy
- ✅ PostgreSQL persistence
- ✅ Redis cache
- ✅ RabbitMQ message queue

---

## 📝 Faydalı Komutlar

```bash
# Container'a bash ile bağlan
docker-compose exec api bash

# API içinde komut çalıştır
docker-compose exec api dotnet ef database update

# Container'ı yeniden build et
docker-compose build --no-cache api

# Tüm container'ları yeniden build et
docker-compose build --no-cache

# Resource kullanımını gör
docker stats
```

---

## 🎯 Normal Run vs Docker

| Özellik | Normal Run | Docker |
|---------|------------|--------|
| PostgreSQL Kurulumu | Manuel | Otomatik |
| Migration | Manuel | Otomatik |
| Port | 5074, 5248 | 5001, 8080 |
| Bağımlılıklar | Elle kur | Otomatik |
| İzolasyon | Yok | Var |
| Production-ready | Hayır | Evet |

**Önerimiz:** Development için Docker, production için de Docker! 🐳

---

## ✅ Kontrol Listesi

Başarılı başlatma için:

- [x] Docker Desktop kurulu ve çalışıyor
- [x] `docker-kontrol.bat` ✅ diyor
- [x] `baslat.bat` çalıştırıldı
- [x] Container'lar başladı (docker-compose ps)
- [x] http://localhost:8080 açılıyor
- [x] Admin girişi yapılabiliyor

---

## 📞 Yardım

Sorun yaşarsanız:

1. Container loglarını kontrol et: `docker-compose logs`
2. Container durumlarını kontrol et: `docker-compose ps`
3. Docker Desktop'ı yeniden başlat
4. `durdur.bat` sonra `baslat.bat` çalıştır

**Hala sorun varsa:**
- README.md
- BASLATMA_ADIMLARI.md
- EKSIKLER_VE_HATALAR.md

---

## 🎉 Başarılı Çalıştırma

Eğer:
- ✅ http://localhost:8080 açılıyorsa
- ✅ Admin girişi yapabiliyorsanız
- ✅ Tüm sayfalar yükleniyorsa

**TEBRİKLER! Docker ile başarıyla çalıştırdınız!** 🚀

