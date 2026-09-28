# AGENTS.md

Bu repo, GitHub issue'larından otomatik çalışan bir kodlama ajanı tarafından da düzenlenir. Ajan bu dosyadaki kurallara uymalıdır.

## Proje

- ASP.NET Core 8 MVC kişisel portfolyo sitesi (`My-Portfolyo.csproj`), canlı adres: https://omerfarukturegun.com.tr
- Klasörler: `Controllers/`, `Models/`, `Services/` (`AuthService`, `JsonFileService`), `Views/` (Razor), `wwwroot/` (statik dosyalar)
- İçerik veritabanında değil JSON dosyalarındadır: Türkçe içerik `wwwroot/data/*.json`, İngilizce içerik `wwwroot/data-en/*.json`
- Routing TR/EN: `/{lang=tr}/{controller}/{action}`

## Kurallar

- Arayüzde görünen her metin değişikliğini hem Türkçe hem İngilizce karşılığıyla birlikte yap.
- `wwwroot/data`, `wwwroot/data-en`, `wwwroot/css` ve `wwwroot/img` canlıda admin panelinden de değiştirilir. Issue açıkça istemedikçe bu klasörlerdeki içeriğe dokunma; `*.backup.*` dosyalarını asla düzenleme.
- Issue açıkça istemedikçe `.env`, `Dockerfile`, `docker-compose*.yml`, `.github/` ve `AGENTS.md` dosyalarını değiştirme.
- Gizli bilgi (şifre, token, API key) ekleme veya loglama.
- Değişikliği issue'nun kapsamıyla sınırlı tut; ilgisiz refactor veya formatlama yapma.
- Mevcut kod stiline (isimlendirme, Bootstrap 5 + jQuery yapısı) uy.

## Doğrulama

İş bitmeden önce derlemenin geçtiğini kontrol et:

```bash
dotnet build My-Portfolyo.csproj -c Release -nologo
```

Commit, push veya PR açma işini yapma; bunları workflow üstlenir.
