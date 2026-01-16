# 📝 Şimdi Ne Yapmalıyım? - Adım Adım Kılavuz

## 🎯 Durumunuz

✅ Tüm dosyalar hazır:
- `docker-compose.yml` (Production)
- `docker-compose.dev.yml` (Development)
- `Dockerfile.dev` (Development için)
- Script dosyaları

⚠️ Mevcut durum:
- Eski production container çalışıyor (`portfolyo-portfolio-web-1`)
- Yeni yapılandırmaya geçmeniz gerekiyor

---

## 🚀 ŞİMDİ YAPMANIZ GEREKENLER

### ADIM 1: Mevcut Production Container'ını Güncelleme

Eski container'ı durdurup yeni yapılandırmaya geçin:

```bash
# 1. Eski container'ı durdur
docker compose down

# 2. Yeni production container'ını başlat (yeni isim: portfolio-web-prod)
docker compose up -d --build
```

**⚠️ DİKKAT:** Bu işlem sırasında site birkaç saniye offline olabilir. 
Önerilen: Düşük trafik saatlerinde yapın veya maintenance window belirleyin.

**Kontrol:**
```bash
# Container'ın çalıştığını kontrol et
docker ps | grep portfolio-web-prod

# Logları kontrol et
docker compose logs portfolio-web-prod
```

---

### ADIM 2: Development Ortamını Test Etme

Development ortamını başlatıp hot-reload'ı test edin:

```bash
# Development container'ını başlat
docker compose -f docker-compose.dev.yml up -d --build

# Logları izle (hot-reload çıktılarını görmek için)
docker compose -f docker-compose.dev.yml logs -f portfolio-web-dev
```

**Tarayıcıda test edin:**
- Development: `http://localhost:5002`
- Production: `http://localhost:5001` (veya `omer.faruk.turegun.com.tr`)

---

### ADIM 3: Hot-Reload'ı Test Etme

1. Development container'ı çalışıyor olmalı
2. Bir dosya düzenleyin (örn: `Views/Home/Index.cshtml`)
3. Dosyayı kaydedin
4. Logları izleyin - otomatik rebuild göreceksiniz:
   ```
   watch : File changed: /src/Views/Home/Index.cshtml
   watch : Started
   ```
5. Tarayıcıyı yenileyin (`http://localhost:5002`) - değişiklikler yansır!

---

## ✅ Kontrol Listesi

Şu adımları tamamladınız mı?

- [ ] **ADIM 1:** Production container güncellendi
- [ ] **ADIM 2:** Development ortamı başlatıldı ve test edildi
- [ ] **ADIM 3:** Hot-reload çalışıyor (test edildi)
- [ ] Her iki ortam aynı anda çalışabiliyor

---

## 🎯 Günlük Kullanım Senaryoları

### Senaryo 1: Yeni Özellik Geliştirme

```bash
# 1. Development'ı başlat
docker compose -f docker-compose.dev.yml up -d

# 2. Kod değişikliklerinizi yapın
# 3. Otomatik hot-reload ile test edin (http://localhost:5002)

# 4. Hazır olduğunuzda production'a deploy edin
git add .
git commit -m "Yeni özellik"
git push

# Sunucuda:
docker compose down
git pull
docker compose up -d --build
```

### Senaryo 2: Production'ı Güncelleme

```bash
# 1. Değişiklikleri çek
git pull

# 2. Production'ı yeniden build et
docker compose down
docker compose up -d --build

# 3. Logları kontrol et
docker compose logs -f portfolio-web-prod
```

### Senaryo 3: Her İki Ortamı Aynı Anda Çalıştırma

```bash
# Production (port 5001)
docker compose up -d

# Development (port 5002)
docker compose -f docker-compose.dev.yml up -d

# Her ikisi de çalışıyor! ✅
```

---

## 🔍 Yararlı Komutlar

### Container Durumunu Kontrol

```bash
# Tüm portfolio container'larını listele
docker ps | grep portfolio

# Production durumu
docker compose ps

# Development durumu
docker compose -f docker-compose.dev.yml ps
```

### Logları İzleme

```bash
# Production logs
docker compose logs -f portfolio-web-prod

# Development logs
docker compose -f docker-compose.dev.yml logs -f portfolio-web-dev
```

### Container'ları Durdurma

```bash
# Production'ı durdur
docker compose down

# Development'ı durdur
docker compose -f docker-compose.dev.yml down

# Her ikisini de durdur
docker compose down
docker compose -f docker-compose.dev.yml down
```

---

## ⚠️ Önemli Notlar

1. **Production ve Development aynı anda çalışabilir** (farklı portlar: 5001 ve 5002)

2. **Production'ı değiştirmeden önce mutlaka development'ta test edin**

3. **Hot-reload sadece development'ta çalışır** - Production'da değişiklik yapmak için container'ı yeniden build etmeniz gerekir

4. **Port çakışması olursa:**
   - `docker-compose.yml` veya `docker-compose.dev.yml` dosyalarında port numaralarını değiştirebilirsiniz

5. **Git workflow önerisi:**
   - Development'ta test → Commit → Push → Production'a deploy

---

## 🆘 Sorun Giderme

### Port zaten kullanılıyor

```bash
# Port'u kullanan process'i bul
sudo lsof -i :5001  # Production için
sudo lsof -i :5002  # Development için
```

### Container başlamıyor

```bash
# Logları kontrol et
docker compose logs portfolio-web-prod
docker compose -f docker-compose.dev.yml logs portfolio-web-dev

# Yeniden build et
docker compose build --no-cache
docker compose -f docker-compose.dev.yml build --no-cache
```

### Hot-reload çalışmıyor

```bash
# Development container'ını yeniden başlat
docker compose -f docker-compose.dev.yml restart
```

---

## 📚 Daha Fazla Bilgi

- Detaylı development kılavuzu: `DEVELOPMENT.md`
- Genel README: `README.md`
- Setup kılavuzu: `SETUP_GUIDE.md`
