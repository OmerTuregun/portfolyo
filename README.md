# Kişisel Portfolyo Sitem (ASP.NET Core & Docker)

Bu proje, ASP.NET Core MVC, Bootstrap 5 ve modern CSS teknikleri kullanılarak oluşturulmuş, Docker üzerinde çalışmak üzere tasarlanmış kişisel bir portfolyo web sitesidir.

# ✨ Öne Çıkan Özellikler

Modern Arayüz: "Glassmorphism" (Cam Efekti) ve canlı "fıstık yeşili" renk paleti.

Tamamen Responsive: Mobil, tablet ve masaüstü cihazlarla uyumludur.

Dinamik Animasyonlar: Sayfa kaydırıldığında dolan yetenek barları gibi interaktif öğeler.

Docker Desteği: Proje, docker-compose ile tek komutta ayağa kalkacak şekilde yapılandırılmıştır.

# 🚀 Projeyi Çalıştırma

Bu proje, **Production** ve **Development** ortamları için ayrı Docker yapılandırmaları içerir. Canlı siteyi bozmadan geliştirme yapabilirsiniz.

## 📋 Ön Gereksinimler

- Docker Desktop kurulu ve çalışır durumda olmalıdır
- .NET 8.0 SDK (development için opsiyonel, Docker içinde mevcut)

---

## 🏭 PRODUCTION Ortamı (Canlı Site)

Production ortamı, canlı site (`omer.faruk.turegun.com.tr`) için optimize edilmiş, Release modunda çalışır.

### Production'ı Çalıştırma:

```bash
# Production container'ını başlat
docker-compose up -d --build

# Logları izle
docker-compose logs -f portfolio-web-prod

# Container'ı durdur
docker-compose down
```

**Erişim:** `http://localhost:5001`

**Özellikler:**
- ✅ Release modunda derlenmiş (optimize)
- ✅ Production environment
- ✅ Restart policy: `unless-stopped`
- ✅ Container adı: `portfolio-web-prod`

---

## 💻 DEVELOPMENT Ortamı (Geliştirme)

Development ortamı, hot-reload ve hızlı geliştirme için tasarlanmıştır. Kod değişiklikleri otomatik olarak yansır.

### Development'ı Çalıştırma:

```bash
# Development container'ını başlat
docker-compose -f docker-compose.dev.yml up -d --build

# Logları izle (hot-reload çıktılarını görmek için)
docker-compose -f docker-compose.dev.yml logs -f portfolio-web-dev

# Container'ı durdur
docker-compose -f docker-compose.dev.yml down
```

**Erişim:** `http://localhost:5002`

**Özellikler:**
- ✅ Hot-reload (kod değişiklikleri otomatik yansır)
- ✅ Development environment
- ✅ Volume mapping (source code mount edilmiş)
- ✅ Watch mode aktif
- ✅ Container adı: `portfolio-web-dev`
- ✅ Farklı port (5002) - production'ı etkilemez

### Development Workflow:

1. **Development container'ını başlat:**
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

2. **Kod değişikliklerinizi yapın** (örneğin: `Views/Home/Index.cshtml`)

3. **Değişiklikler otomatik yansır!** Tarayıcıyı yenileyin.

4. **Test edin:** `http://localhost:5002`

5. **Hazır olduğunuzda production'a deploy edin**

---

## 🔄 Production'a Deploy Etme

### Güvenli Deploy Adımları:

1. **Development'ta test edin:**
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   # http://localhost:5002'de test edin
   ```

2. **Değişiklikleri commit edin:**
   ```bash
   git add .
   git commit -m "Yeni özellik eklendi"
   git push
   ```

3. **Production'ı güncelleyin:**
   ```bash
   # Production container'ını durdur
   docker-compose down
   
   # Yeni değişiklikleri çek (git pull)
   git pull
   
   # Production'ı yeniden build et ve başlat
   docker-compose up -d --build
   ```

4. **Production loglarını kontrol edin:**
   ```bash
   docker-compose logs -f portfolio-web-prod
   ```

---

## 📁 Dosya Yapısı

```
portfolyo/
├── docker-compose.yml          # PRODUCTION için
├── docker-compose.dev.yml      # DEVELOPMENT için
├── Dockerfile                   # PRODUCTION build
├── Dockerfile.dev              # DEVELOPMENT build (hot-reload)
└── ...
```

---

## 🛠️ Yararlı Komutlar

### Container Durumunu Kontrol Etme:

```bash
# Tüm container'ları listele
docker ps -a

# Production container'ını kontrol et
docker ps | grep portfolio-web-prod

# Development container'ını kontrol et
docker ps | grep portfolio-web-dev
```

### Logları İzleme:

```bash
# Production logs
docker-compose logs -f portfolio-web-prod

# Development logs
docker-compose -f docker-compose.dev.yml logs -f portfolio-web-dev
```

### Container'a Bağlanma:

```bash
# Production container'ına bash ile bağlan
docker exec -it portfolio-web-prod bash

# Development container'ına bash ile bağlan
docker exec -it portfolio-web-dev bash
```

### Container'ları Temizleme:

```bash
# Production'ı durdur ve sil
docker-compose down

# Development'ı durdur ve sil
docker-compose -f docker-compose.dev.yml down

# Tüm container'ları, image'ları ve volume'ları temizle (DİKKAT!)
docker-compose down -v --rmi all
docker-compose -f docker-compose.dev.yml down -v --rmi all
```

---

## ⚠️ Önemli Notlar

1. **Production ve Development aynı anda çalışabilir** (farklı portlar: 5001 ve 5002)

2. **Production'ı değiştirmeden önce mutlaka development'ta test edin**

3. **Git workflow kullanın:** Development'ta test → Commit → Production'a deploy

4. **Production container'ı `unless-stopped` policy ile çalışır** (sunucu restart olsa bile otomatik başlar)

5. **Development'ta hot-reload aktif** - Kod değişiklikleri otomatik yansır, container'ı yeniden başlatmanıza gerek yok

---

## 🐛 Sorun Giderme

### Port zaten kullanılıyor hatası:

```bash
# Port'u kullanan process'i bul
sudo lsof -i :5001  # Production için
sudo lsof -i :5002  # Development için

# Veya docker-compose.yml'de port'u değiştirin
```

### Container başlamıyor:

```bash
# Logları kontrol edin
docker-compose logs portfolio-web-prod
docker-compose -f docker-compose.dev.yml logs portfolio-web-dev

# Container'ı yeniden build edin
docker-compose build --no-cache
docker-compose -f docker-compose.dev.yml build --no-cache
```

### Hot-reload çalışmıyor:

```bash
# Development container'ını yeniden başlat
docker-compose -f docker-compose.dev.yml restart
```