# PostgreSQL Kurulum Rehberi

## ❗ ÖNEMLİ: PostgreSQL Kurulu Değil!

API'nin çalışması için PostgreSQL veritabanı gereklidir.

---

## 📥 PostgreSQL Kurulumu

### Yöntem 1: Windows Installer (Önerilen)

1. **İndir:**
   - https://www.postgresql.org/download/windows/
   - veya direkt: https://www.enterprisedb.com/downloads/postgres-postgresql-downloads
   - **Önerilen Versiyon:** PostgreSQL 15 veya 16

2. **Kur:**
   - Installer'ı çalıştır
   - **Port:** 5432 (varsayılan)
   - **Superuser Şifresi:** Unutmayacağınız bir şifre belirleyin (örn: `postgres123`)
   - Stack Builder'ı çalıştırmayı seçmeyin (ihtiyaç yok)

3. **Kontrol Et:**
   ```bash
   psql --version
   ```
   Çıktı: `psql (PostgreSQL) 15.x` gibi olmalı

### Yöntem 2: Docker (Alternatif)

Docker Desktop varsa:

1. **Docker Desktop'ı Başlat**

2. **PostgreSQL Container Çalıştır:**
   ```bash
   docker run -d ^
     --name postgres-eticaret ^
     -e POSTGRES_PASSWORD=postgres123 ^
     -e POSTGRES_USER=postgres ^
     -e POSTGRES_DB=postgres ^
     -p 5432:5432 ^
     postgres:15
   ```

3. **Kontrol Et:**
   ```bash
   docker ps | findstr postgres
   ```

---

## 🗄️ Database Kurulumu

PostgreSQL kurduktan sonra:

### Otomatik Kurulum (Önerilen)

```bash
cd D:\ETicaret\OZELDERS
setup-database.bat
```

Bu script:
- ✅ `ETicaret` database'ini oluşturur
- ✅ `ETicaret_user` kullanıcısını oluşturur
- ✅ Gerekli yetkileri verir

### Manuel Kurulum

1. **PostgreSQL'e Bağlan:**
   ```bash
   psql -U postgres
   ```
   Şifrenizi girin (kurulumda belirlediğiniz)

2. **Database Oluştur:**
   ```sql
   CREATE DATABASE "ETicaret";
   ```

3. **Kullanıcı Oluştur:**
   ```sql
   CREATE USER ETicaret_user WITH PASSWORD 'OzelDers_Dev_2024!';
   ```

4. **Yetkileri Ver:**
   ```sql
   GRANT ALL PRIVILEGES ON DATABASE "ETicaret" TO ETicaret_user;
   \c ETicaret
   GRANT ALL ON SCHEMA public TO ETicaret_user;
   ```

5. **Çık:**
   ```sql
   \q
   ```

---

## 🔧 Migration Uygulama

Database oluşturduktan sonra:

```bash
cd D:\ETicaret\OZELDERS\src\ETicaret.API
dotnet ef database update
```

Bu komut:
- ✅ Tabloları oluşturur
- ✅ Seed data'yı yükler (admin kullanıcısı, kategoriler, vs.)

---

## ✅ Kontrol

### 1. PostgreSQL Servisi Çalışıyor mu?

**Windows:**
```bash
# Servis kontrol
sc query postgresql-x64-15

# veya Services.msc'yi aç
# "postgresql-x64-15" servisini bul
```

**Docker:**
```bash
docker ps | findstr postgres
```

### 2. Port Dinleniyor mu?

```bash
netstat -ano | findstr :5432
```

Çıktı varsa PostgreSQL çalışıyor.

### 3. Bağlantı Testi

```bash
cd D:\ETicaret\OZELDERS
test-baglanti.bat
```

veya manuel:
```bash
psql -h localhost -p 5432 -U ETicaret_user -d ETicaret -c "\dt"
```

---

## 🚀 Projeyi Başlatma

PostgreSQL kurulumu tamamlandıktan sonra:

1. **Migration Çalıştır:**
   ```bash
   cd src\ETicaret.API
   dotnet ef database update
   ```

2. **Servisleri Başlat:**
   ```bash
   run-all.bat
   ```

3. **Test Et:**
   - Web: http://localhost:5248
   - API: http://localhost:5074/swagger

---

## ❓ Sorun Giderme

### Hata: "psql: command not found"

**Çözüm:** PostgreSQL PATH'e eklenmemiş.

1. PostgreSQL kurulum klasörünü bul:
   ```
   C:\Program Files\PostgreSQL\15\bin
   ```

2. System Environment Variables'a ekle:
   - Windows Ayarlar → Sistem → Gelişmiş Sistem Ayarları
   - Environment Variables → System Variables → Path
   - Yeni → `C:\Program Files\PostgreSQL\15\bin`

3. Terminal'i yeniden başlat

### Hata: "password authentication failed"

**Çözüm:** Şifre yanlış veya kullanıcı yok.

1. PostgreSQL şifresini hatırla (kurulumda belirlediğin)
2. `setup-database.bat` ile yeniden kur
3. Manuel kurulumu dene (yukarıda)

### Hata: "database does not exist"

**Çözüm:** Database henüz oluşturulmamış.

```bash
setup-database.bat
```

### Hata: "port 5432 already in use"

**Çözüm:** Başka bir PostgreSQL instance çalışıyor.

1. Çalışan process'i bul:
   ```bash
   netstat -ano | findstr :5432
   ```

2. Process ID'yi not al ve kapat:
   ```bash
   taskkill /PID [PID] /F
   ```

---

## 📝 Bağlantı Bilgileri

Projenizde kullanılan bağlantı bilgileri:

```
Host: localhost
Port: 5432
Database: ETicaret
Username: ETicaret_user
Password: OzelDers_Dev_2024!
```

Bu bilgiler şurada tanımlı:
```
D:\ETicaret\OZELDERS\src\ETicaret.API\appsettings.Development.json
```

---

## 🎯 Özet

1. ✅ PostgreSQL kur (Windows Installer veya Docker)
2. ✅ `setup-database.bat` çalıştır
3. ✅ `dotnet ef database update` çalıştır
4. ✅ `run-all.bat` ile başlat

**Sorun yaşarsan:** Bu dosyayı oku, adım adım takip et!

