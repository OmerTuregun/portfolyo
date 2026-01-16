# 💻 Development Kılavuzu

Bu dosya, projeyi geliştirme ortamında çalıştırma ve geliştirme yapma konusunda detaylı bilgiler içerir.

## 🎯 Hızlı Başlangıç

```bash
# 1. Development container'ını başlat
docker-compose -f docker-compose.dev.yml up -d --build

# 2. Logları izle (hot-reload çıktılarını görmek için)
docker-compose -f docker-compose.dev.yml logs -f

# 3. Tarayıcıda aç: http://localhost:5002
```

## 🔥 Hot-Reload Özelliği

Development ortamında **hot-reload** aktif. Bu demek oluyor ki:

- ✅ Kod değişiklikleriniz otomatik olarak yansır
- ✅ Container'ı yeniden başlatmanıza gerek yok
- ✅ Sadece tarayıcıyı yenileyin

### Hot-Reload Nasıl Çalışır?

1. `Dockerfile.dev` içinde `dotnet watch` kullanılır
2. Source code volume mount edilir (`-v .:/src`)
3. Dosya değişiklikleri otomatik algılanır
4. Uygulama otomatik yeniden derlenir ve başlatılır

### Hangi Dosyalar Hot-Reload Destekler?

- ✅ `.cs` dosyaları (Controllers, Models)
- ✅ `.cshtml` dosyaları (Views)
- ✅ `.json` dosyaları (appsettings.json)
- ✅ `.css` dosyaları (wwwroot/css)
- ✅ `.js` dosyaları (wwwroot/js)

## 📝 Geliştirme Workflow'u

### 1. Yeni Özellik Geliştirme

```bash
# Development container'ını başlat
docker-compose -f docker-compose.dev.yml up -d

# Yeni bir branch oluştur (opsiyonel ama önerilir)
git checkout -b feature/yeni-ozellik

# Kod değişikliklerinizi yapın
# Örnek: Views/Home/Index.cshtml dosyasını düzenleyin

# Tarayıcıda test edin: http://localhost:5002
# Değişiklikler otomatik yansır!
```

### 2. Test Etme

```bash
# Logları izleyerek hataları görebilirsiniz
docker-compose -f docker-compose.dev.yml logs -f portfolio-web-dev

# Container içine girip manuel test yapabilirsiniz
docker exec -it portfolio-web-dev bash
```

### 3. Production'a Deploy

```bash
# 1. Değişiklikleri commit edin
git add .
git commit -m "Yeni özellik eklendi"
git push

# 2. Production'ı güncelleyin (sunucuda)
docker-compose down
git pull
docker-compose up -d --build
```

## 🛠️ Geliştirme Araçları

### Visual Studio Code ile Geliştirme

VS Code'da projeyi açtığınızda:

1. **C# Extension** yüklü olmalı
2. **Docker Extension** yüklü olmalı (opsiyonel)
3. IntelliSense çalışacak

### Debugging

Development container'ına bağlanarak debug yapabilirsiniz:

```bash
# Container'a bağlan
docker exec -it portfolio-web-dev bash

# Dotnet CLI komutlarını kullanabilirsiniz
dotnet --version
dotnet build
dotnet run
```

## 📦 Bağımlılıkları Güncelleme

Eğer `.csproj` dosyasına yeni bir NuGet paketi eklerseniz:

```bash
# Development container'ını yeniden build et
docker-compose -f docker-compose.dev.yml build --no-cache
docker-compose -f docker-compose.dev.yml up -d
```

## 🔍 Sorun Giderme

### Hot-Reload Çalışmıyor

```bash
# Container'ı yeniden başlat
docker-compose -f docker-compose.dev.yml restart

# Veya tamamen yeniden build et
docker-compose -f docker-compose.dev.yml down
docker-compose -f docker-compose.dev.yml up -d --build
```

### Port 5002 Zaten Kullanılıyor

```bash
# Port'u kullanan process'i bul
sudo lsof -i :5002

# Veya docker-compose.dev.yml'de port'u değiştirin
# ports:
#   - "5003:5002"  # 5003'e değiştir
```

### Volume Mount Sorunları

Eğer dosya değişiklikleri yansımıyorsa:

```bash
# Container'ı durdur
docker-compose -f docker-compose.dev.yml down

# Volume'ları temizle (DİKKAT: Bu tüm data'yı siler)
docker volume prune

# Yeniden başlat
docker-compose -f docker-compose.dev.yml up -d --build
```

## 🎨 Frontend Geliştirme

### CSS/JS Dosyalarını Düzenleme

`wwwroot/css/site.css` veya `wwwroot/js/site.js` dosyalarını düzenlediğinizde:

1. Dosyayı kaydedin
2. Tarayıcıda hard refresh yapın (Ctrl+Shift+R veya Cmd+Shift+R)
3. Değişiklikler yansır

### View Dosyalarını Düzenleme

`.cshtml` dosyalarını düzenlediğinizde:

1. Dosyayı kaydedin
2. Hot-reload otomatik çalışır
3. Tarayıcıyı yenileyin

## 📊 Performance İpuçları

Development ortamı production'dan daha yavaş olabilir çünkü:

- Debug modunda çalışır
- Hot-reload overhead'i vardır
- Optimize edilmemiş build kullanır

Bu normaldir. Production'da performans çok daha iyi olacaktır.

## 🔐 Güvenlik Notları

- Development ortamı sadece **localhost**'ta çalışmalıdır
- Production'da kullanılan environment variables'ları development'ta kullanmayın
- `.env` dosyasını asla commit etmeyin (`.gitignore`'da olmalı)

## 📚 Ek Kaynaklar

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
