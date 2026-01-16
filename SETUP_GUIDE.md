# 🚀 Kurulum ve Kullanım Kılavuzu

Bu kılavuz, production ve development ortamlarını ayarlama ve kullanma adımlarını içerir.

## 📋 Adım Adım Kurulum

### 1️⃣ Mevcut Production Container'ını Güncelleme

Mevcut production container'ınız eski isimle çalışıyor. Yeni yapılandırmaya geçmek için:

```bash
# Eski container'ı durdur
docker-compose down

# Yeni production container'ını başlat (yeni isim: portfolio-web-prod)
docker-compose up -d --build
```

**Not:** Bu işlem sırasında site birkaç saniye offline olabilir. Önerilen: Düşük trafik saatlerinde yapın.

### 2️⃣ Development Ortamını Test Etme

Development ortamını başlatıp test edin:

```bash
# Development container'ını başlat
docker-compose -f docker-compose.dev.yml up -d --build

# Logları kontrol et
docker-compose -f docker-compose.dev.yml logs -f portfolio-web-dev
```

Tarayıcıda açın: `http://localhost:5002`

### 3️⃣ Hot-Reload'ı Test Etme

1. Development container'ı çalışıyor olmalı
2. Bir view dosyasını düzenleyin (örn: `Views/Home/Index.cshtml`)
3. Dosyayı kaydedin
4. Logları izleyin - otomatik rebuild göreceksiniz
5. Tarayıcıyı yenileyin - değişiklikler yansır!

## 🎯 Günlük Kullanım

### Development'ta Çalışma

```bash
# Development'ı başlat
./scripts/dev-start.sh
# veya
docker-compose -f docker-compose.dev.yml up -d

# Logları izle
docker-compose -f docker-compose.dev.yml logs -f

# Development'ı durdur
./scripts/dev-stop.sh
# veya
docker-compose -f docker-compose.dev.yml down
```

### Production'a Deploy

```bash
# 1. Development'ta test et (http://localhost:5002)
# 2. Değişiklikleri commit et
git add .
git commit -m "Yeni özellik"
git push

# 3. Production'ı güncelle (sunucuda)
./scripts/prod-start.sh
# veya
docker-compose down
git pull
docker-compose up -d --build
```

## ✅ Kontrol Listesi

- [ ] Production container güncellendi (`portfolio-web-prod`)
- [ ] Development ortamı test edildi (`http://localhost:5002`)
- [ ] Hot-reload çalışıyor
- [ ] Her iki ortam aynı anda çalışabiliyor
- [ ] Script dosyaları çalıştırılabilir (`chmod +x scripts/*.sh`)

## 🔍 Durum Kontrolü

```bash
# Tüm container'ları listele
docker ps -a | grep portfolio

# Production durumu
docker-compose ps

# Development durumu
docker-compose -f docker-compose.dev.yml ps
```

## ⚠️ Önemli Notlar

1. **Production ve Development aynı anda çalışabilir** (farklı portlar)
2. **Production'ı değiştirmeden önce mutlaka development'ta test edin**
3. **Development port: 5002, Production port: 5001**
4. **Hot-reload sadece development'ta çalışır**
