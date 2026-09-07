# Hangi Başlatma Yöntemini Kullanmalıyım?

## 🔄 Durum: Development (Geliştirme) Yapıyorsunuz

### ❌ Docker Kullanmamalısınız Çünkü:

1. **Her değişiklikte rebuild gerekiyor** (5-10 dakika)
2. **CSS değişiklikleri görünmüyor** (production build)
3. **Yeni sayfalar görünmüyor** (image eski)
4. **Debug zorlaşıyor** (container içinde)

### ✅ Normal Run Kullanmalısınız:

1. **Anında değişiklikler** (hot reload)
2. **CSS anında yüklenir**
3. **Yeni sayfalar hemen görünür**
4. **Debug kolay**

---

## 🚀 Doğru Yöntem: Normal Run

### Seçenek 1: PostgreSQL Kurulu (Önerilen)

**Avantajları:**
- ✅ En hızlı
- ✅ Hot reload çalışır
- ✅ Her değişiklik anında görünür

**Dezavantajları:**
- ❌ PostgreSQL kurmanız gerekiyor

**Başlatma:**
```bash
# Sadece bir kez (ilk kurulum)
setup-database.bat
cd src\ETicaret.API
dotnet ef database update

# Her seferinde
run-all.bat
```

### Seçenek 2: Docker Sadece PostgreSQL

PostgreSQL için Docker, API/Web için normal run:

```bash
# docker-compose.yml düzenle - sadece postgres kalsın

# PostgreSQL başlat
docker-compose up -d postgres

# API ve Web'i normal başlat  
run-all.bat
```

### Seçenek 3: Tam Docker (Sadece Production Test İçin)

**Ne zaman kullanmalı:** Production'a deploy etmeden önce test için

**Başlatma:**
```bash
yeniden-basla.bat  # Her değişiklikten sonra!
```

---

## 📊 Karşılaştırma

| Özellik | Normal Run | Tam Docker |
|---------|------------|-----------|
| **İlk başlatma** | 10-20 saniye | 5-10 dakika |
| **Kod değişikliği sonrası** | Anında | 5-10 dakika rebuild |
| **CSS değişikliği** | Anında | Rebuild gerekli |
| **Yeni sayfa ekleme** | Anında | Rebuild gerekli |
| **Hot Reload** | ✅ Var | ❌ Yok |
| **Debug** | ✅ Kolay | ❌ Zor |
| **PostgreSQL kurulumu** | ⚠️ Gerekli | ✅ Otomatik |
| **Production test** | ❌ | ✅ |

---

## 🎯 ŞU ANKİ DURUMUNUZ

### Sorununuz:
```
❌ Eklediğiniz sayfalar görünmüyor
❌ CSS yüklenmiyor  
❌ Adreslerim hata veriyor
❌ Font bozuk
```

### Nedeni:
```
Docker eski image ile çalışıyor
Yeni kodunuz container'da yok
```

### Çözüm:

**SEÇENEK A: Docker ile devam et (UZUN)**
```bash
# 5-10 dakika sürer
yeniden-basla.bat
```

**SEÇENEK B: Normal run'a geç (HIZLI - ÖNERİLEN)**
```bash
# Container'ları durdur
docker-compose down

# PostgreSQL kur (sadece bir kez)
setup-database.bat

# Migration (sadece bir kez)
cd src\ETicaret.API
dotnet ef database update

# Normal başlat
cd ..\..
run-all.bat
```

**SEÇENEK C: Hibrit (ORTA)**
```bash
# Sadece PostgreSQL Docker'da
docker-compose up -d postgres

# API ve Web normal
run-all.bat
```

---

## 💡 ÖNERİM

**Development için:** SEÇENEK B (Normal Run)
- PostgreSQL'i bir kez kur
- Her değişikliği anında gör
- Hızlı geliştir

**Production test için:** SEÇENEK A (Docker)
- Deploy etmeden önce test et
- Tam izolasyon
- Production ortamına yakın

---

## ⚡ HIZLI ÇÖZÜM (ŞİMDİ NE YAPAYIM?)

### Şu an Docker çalışıyor ama sayfalar boş:

```bash
# 1. Docker'ı durdur
docker-compose down

# 2. PostgreSQL kontrolü
# Eğer PostgreSQL kuruluysa:
run-all.bat

# Eğer PostgreSQL kurulu değilse:
setup-database.bat
# sonra
run-all.bat
```

**10 saniye içinde çalışır! ✅**

---

## 🤔 Hangisini Seçmeliyim?

**Sorunuz:** PostgreSQL kurulu mu?

✅ **Evet kurulu** → `run-all.bat` kullan (EN HIZLI)

❌ **Hayır kurulu değil ve kurmak istemiyorum** → `yeniden-basla.bat` kullan (5-10 dakika bekle)

❌ **Hayır ama kuruyum** → `setup-database.bat` sonra `run-all.bat` kullan (İLK SEFERDE 2 dakika, sonra hep 10 saniye)

---

## 📝 Özet

**Development yaparken:** Docker kullanma! 
**Production test ederken:** Docker kullan!

**Şu anki sorunun:** Docker production build kullanıyor, dev değişikliklerini görmüyor.

**En hızlı çözüm:** `docker-compose down` sonra `run-all.bat`

