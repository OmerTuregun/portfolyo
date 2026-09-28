# Kişisel Portfolyo (ASP.NET Core MVC)

Ömer Faruk Türegün kişisel portfolyo sitesi: anasayfa, projeler, deneyim, hakkımda, iletişim. TR/EN dil desteği. İçerik JSON dosyalarından okunur (ayrı veritabanı yok). Session + BCrypt ile admin paneli üzerinden proje ve deneyim CRUD yapılır.

**Canlı site:** https://omerfarukturegun.com.tr  
**Sunucu yolu:** `/root/portfolyo`  
Compose proje adı: **`portfolyo`** (container: `portfolio-web-prod`)

## Teknoloji yığını

| Katman | Teknoloji |
|--------|-----------|
| Uygulama | ASP.NET Core 8 MVC (`My-Portfolyo.csproj`) |
| UI | Bootstrap 5, jQuery, özel CSS/JS (glassmorphism) |
| İçerik | `wwwroot/data/*.json` ve `wwwroot/data-en/*.json` |
| Auth | Session (30 dk, HttpOnly, SameSite=Strict) + BCrypt (`AuthService`) |
| Altyapı | Docker Compose, host nginx reverse proxy + Let's Encrypt |

## Özellikler

- Tamamen responsive; TR/EN routing: `/{lang=tr}/{controller}/{action}` (`tr` veya `en`)
- Sayfalar: Index, Projects, Experience, AboutMe, Contact, Privacy
- Admin: `/Admin/Login` → dashboard, projeler ve deneyim (eğitim / iş / staj / dil) CRUD
- `JsonFileService` yazarken timestamped yedek alır
- Production Release image; development `dotnet watch` hot-reload

## Canlı mimari

```
İnternet (HTTPS)
  → host nginx  omerfarukturegun.com.tr
       → 127.0.0.1:5001  portfolio-web-prod
```

| Ortam | Compose | Container | Port |
|-------|---------|-----------|------|
| Production | `docker-compose.yml` | `portfolio-web-prod` | **`127.0.0.1:5001`** |
| Development | `docker-compose.dev.yml` | `portfolio-web-dev` | **`0.0.0.0:3000`** |

> Eski dokümanlardaki development portu **5002** güncel değildir. Compose ve `scripts/dev-start.sh` **3000** kullanır.

## Production

```bash
cd /root/portfolyo
docker compose up -d --build
docker compose logs -f portfolio-web-prod
```

- `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS=http://+:5001`
- Restart: `unless-stopped`
- Volume yok (içerik image içinde; admin kaydı image'a yazılır — kalıcılık için volume veya bind mount düşünülmeli)
- Labels: `com.omer.project=portfolio`

Durdurma:

```bash
docker compose down
# veya
./scripts/prod-stop.sh
```

## Development

```bash
cd /root/portfolyo
docker compose -f docker-compose.dev.yml up -d --build
# http://localhost:3000
```

- Source mount (`.:/src`), `dotnet watch run`
- `.env` read-only mount
- Production ile aynı anda çalışabilir (farklı port)

Yardımcı scriptler: `scripts/dev-start.sh`, `dev-stop.sh`, `prod-start.sh`, `prod-stop.sh`.

## Ortam değişkenleri

`.env` (git'e commit edilmez):

| Değişken | Amaç |
|----------|------|
| `ADMIN_USERNAME` | Admin paneli kullanıcı adı |
| `ADMIN_PASSWORD` | BCrypt hash (düz metin fallback uyarısı vardır) |

`Program.cs` DotNetEnv ile `.env` yükler.

## Proje yapısı

```
portfolyo/
├── docker-compose.yml          # production
├── docker-compose.dev.yml      # development (port 3000)
├── Dockerfile / Dockerfile.dev
├── Program.cs
├── Controllers/                # Home, Admin, AdminProjects, AdminExperience
├── Models/  Views/  Services/  # JsonFileService, AuthService
├── Attributes/AdminAuthorizeAttribute.cs
├── wwwroot/data  wwwroot/data-en
├── scripts/
├── ADIM_ADIM.md
├── DEVELOPMENT.md
└── SETUP_GUIDE.md
```

> `ADIM_ADIM.md`, `DEVELOPMENT.md` ve `SETUP_GUIDE.md` içinde geçen **5002** ve `omer.faruk.turegun.com.tr` ifadeleri eskidir. Güncel: **dev 3000**, **prod 5001**, domain **omerfarukturegun.com.tr**.

## Yönetim

```bash
docker ps | grep portfolio-web-prod
docker compose logs -f portfolio-web-prod
docker exec -it portfolio-web-prod bash
```
