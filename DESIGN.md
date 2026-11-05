# DESIGN.md — "Bento" Tasarım Sistemi

> omerfarukturegun.com.tr yeniden tasarımının tek doğruluk kaynağı.
> Bu dosya yeni arayüzün renklerini, tipografisini, bileşenlerini, hareket kurallarını ve sayfa yapılarını tanımlar.
> Kod yazarken (insan ya da AI ajanı) bu dosyadaki değerlerin dışına çıkma; yeni bir değer gerekiyorsa önce buraya ekle.

---

## 1. Tasarım fikri

**Açık renkli bento grid + tek turuncu vurgu + monospace detaylar.**

- Sayfalar, yuvarlak köşeli beyaz **karolardan (tile)** oluşan bir ızgaradır. Her karo tek bir fikri anlatır (tanıtım, şu anki iş, konum, proje, stack…).
- Zemin sakin açık gri, karolar beyaz. Göz yormayan, bol boşluklu, yüksek kontrastlı metin.
- **Tek vurgu rengi turuncudur** ve az kullanılır: öne çıkan proje karosu, CTA butonu, küçük etiketler, durum noktaları.
- Yazılımcı kimliği "neon terminal" ile değil; **JetBrains Mono** ile yazılmış küçük etiketler, tarihler, teknoloji listeleri ve GitHub katkı grafiği gibi detaylarla verilir.
- Derinlik hissi **hafif 3D hover** ile gelir: karo üzerine gelince `rotateX(3deg)` ile hafifçe yatar ve yükselir. Abartı yok.

**Yapma:** gradyan arka planlar, cam (glassmorphism) efektleri, sol kenar çizgili kartlar, emoji ikonlar, yüzdeli yetenek çubukları, sabit (fixed) footer, sürekli dönen animasyonlar.

---

## 2. Renk token'ları

Tüm renkler `:root` üzerinde CSS değişkeni olarak tanımlanır. Bileşenlerde **ham hex yazma, değişken kullan.**

```css
:root {
  /* Zemin & yüzeyler */
  --bg:            #eef0f3;  /* sayfa zemini */
  --surface:       #ffffff;  /* karo */
  --surface-2:     #f3f4f6;  /* chip, input, ikincil yüzey */
  --border:        #e1e4e8;  /* karo kenarlığı */
  --border-strong: #d3d7dd;  /* ikincil buton kenarı */

  /* Metin */
  --ink:           #111317;  /* ana metin, koyu karo zemini */
  --ink-2:         #3d4450;  /* gövde paragrafları */
  --muted:         #5b6370;  /* etiket, tarih, yardımcı metin (beyaz üstünde 6:1) */

  /* Vurgu (turuncu) */
  --accent:        #f97316;  /* öne çıkan karo, CTA zemin */
  --accent-strong: #ea580c;  /* nokta, pin, logo noktası */
  --accent-ink:    #c2410c;  /* açık zemin üstünde turuncu YAZI (kontrast için) */
  --on-accent:     #1a0d03;  /* turuncu zemin üstündeki yazı */
  --accent-tint:   #fff1e6;  /* turuncu açık ton (AI kategori kapağı, seçili chip) */

  /* Koyu karo */
  --dark:          #111317;
  --dark-2:        #1f2329;
  --on-dark:       #f3f4f6;
  --on-dark-muted: #b8bec8;

  /* Durum */
  --success:       #22c55e;
  --success-tint:  #ecfdf3;
  --success-ink:   #146c3a;

  /* Kategori renkleri (proje kapakları) */
  --cat-ai-tint:   #fff1e6;  --cat-ai-ink:   #c2410c;
  --cat-web-tint:  #eef2ff;  --cat-web-ink:  #3730a3;
  --cat-ind-tint:  #e9f9ef;  --cat-ind-ink:  #146c3a;

  /* GitHub katkı grafiği skalası (az → çok) */
  --heat-0: #eceef1; --heat-1: #fed7aa; --heat-2: #fdba74; --heat-3: #fb923c; --heat-4: #ea580c;
}
```

**Kontrast kuralları**
- Beyaz zemin üstünde turuncu **metin** gerekiyorsa `--accent-ink` (#c2410c) kullan; `--accent` metin için yeterli kontrast vermez.
- Turuncu zemin üstündeki yazı her zaman `--on-accent`.
- 13px altı yazı kullanma (mono etiketler 12px'e kadar inebilir, en az `--muted` renginde).

---

## 3. Tipografi

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
```

```css
:root {
  --font-sans: 'Manrope', system-ui, -apple-system, 'Segoe UI', sans-serif;
  --font-mono: 'JetBrains Mono', ui-monospace, 'SFMono-Regular', Consolas, monospace;
}
html { font-size: 16px; }            /* ASLA !important ile büyütme */
body { font-family: var(--font-sans); font-size: 16px; line-height: 1.55; color: var(--ink); background: var(--bg); }
```

| Rol | Font | Boyut | Ağırlık | Harf aralığı | Satır yüksekliği |
|---|---|---|---|---|---|
| Display (hero h1) | Manrope | `clamp(44px, 5vw, 66px)` | 800 | `-0.045em` | 0.98 |
| Sayfa başlığı (h1) | Manrope | `clamp(40px, 5.4vw, 72px)` | 800 | `-0.05em` | 0.95 |
| Bölüm başlığı (h2) | Manrope | `clamp(28px, 3.6vw, 48px)` | 800 | `-0.04em` | 1.0 |
| Karo başlığı (h3) | Manrope | `19–26px` | 800 | `-0.02em` | 1.2 |
| Lead paragraf | Manrope | `17–19px` | 400 | 0 | 1.55 |
| Gövde | Manrope | `15–16px` | 400–500 | 0 | 1.55 |
| Eyebrow / etiket | JetBrains Mono | `12px` | 400–500 | 0, **BÜYÜK HARF** | 1.4 |
| Tarih, tag listesi | JetBrains Mono | `12–13px` | 400 | 0 | 1.5 |

- Bölüm başlıklarının üstünde her zaman bir **mono eyebrow** olur: `SEÇİLİ İŞLER`, `DENEYİM`, `SERTİFİKALAR` (renk `--accent-ink`).
- Paragraf genişliği en fazla `48ch`.
- Mobilde font büyütme hack'i yok: tüm boyutlar `clamp()` ile akışkandır.

---

## 4. Boşluk, köşe, gölge

```css
:root {
  --space-1: 4px;  --space-2: 8px;  --space-3: 12px; --space-4: 16px;
  --space-5: 22px; --space-6: 28px; --space-7: 40px; --space-8: 64px;

  --radius-sm: 10px;   /* chip */
  --radius-md: 14px;   /* input, küçük buton */
  --radius-lg: 20px;   /* küçük karo, sertifika */
  --radius-xl: 28px;   /* standart karo */
  --radius-2xl: 32px;  /* CTA karosu */
  --radius-pill: 999px;

  --shadow-hover: 0 24px 50px rgba(17, 19, 23, 0.10);
  --container: 1280px;
  --gutter: clamp(16px, 3vw, 40px);
  --gap: 16px;          /* bento aralığı — her yerde aynı */
}
```

- Karolar arası boşluk **her zaman 16px**.
- Bölümler arası dikey boşluk `64px` (mobilde 48px).
- Karo iç boşluğu: standart `26px`, büyük (hero) `clamp(26px, 3vw, 40px)`.

---

## 5. Izgara (Bento)

```css
.container-d { max-width: var(--container); margin: 0 auto; padding: 24px var(--gutter) 40px; }

.bento {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  grid-auto-rows: minmax(210px, auto);
  gap: var(--gap);
}
.span-2 { grid-column: span 2; }
.span-4 { grid-column: span 4; }
.row-2  { grid-row: span 2; }
.row-3  { grid-row: span 3; }

@media (max-width: 1000px) {
  .bento { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .span-4 { grid-column: span 2; }
}
@media (max-width: 640px) {
  .bento { grid-template-columns: minmax(0, 1fr); }
  .span-2, .span-4 { grid-column: auto; }
  .row-2, .row-3 { grid-row: auto; }
}
```

Kart listeleri (projeler) için: `grid-template-columns: repeat(auto-fill, minmax(280px, 1fr))`.

---

## 6. Bileşenler

### 6.1 Karo (`.tile`)
```css
.tile {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  padding: 26px;
  display: flex; flex-direction: column; gap: 10px;
  transition: transform .35s cubic-bezier(.2,.7,.2,1), box-shadow .35s ease;
}
.tile:hover { transform: perspective(1000px) rotateX(3deg) translateY(-5px); box-shadow: var(--shadow-hover); }
.tile--dark   { background: var(--dark); color: var(--on-dark); border: 0; }
.tile--accent { background: var(--accent); color: var(--on-accent); border: 0; }
.tile--static:hover { transform: none; box-shadow: none; } /* form gibi etkileşimli karolar */
```
- Tıklanabilir karo `<a class="tile">` olur; içinde ayrıca buton olmaz.
- Form içeren karo `tile--static` olur (hover'da oynamaz).

### 6.2 Üst menü (pill nav)
- Sol: logo `ömerfaruk` + turuncu `.` (`--accent-strong`), 18px / 800.
- Orta: beyaz pill kapsayıcı (`padding:5px; border:1px solid var(--border); border-radius:999px`); içindeki linkler `padding:9px 16px; border-radius:999px; font-size:14px; font-weight:600`.
- Aktif link: `background: var(--ink); color: #fff;` ve `aria-current="page"`.
- Sağ: `TR / EN` dil değiştirici (aktif dil kalın).
- Menü öğeleri: Ana sayfa · Projeler · Deneyim · Hakkımda · İletişim. Mobilde pill `flex-wrap` ile alt satıra kayar; hamburger yok.

### 6.3 Butonlar
| Tür | Stil |
|---|---|
| Birincil | `background: var(--ink); color:#fff; min-height:50px; padding:0 24px; border-radius:16px; font-weight:700` |
| İkincil | `border:1px solid var(--border-strong); background:transparent; aynı ölçüler; font-weight:600` |
| Vurgu (CTA) | `background: var(--accent); color: var(--on-accent); font-weight:800; border-radius:18px` |

Tüm tıklanabilir öğeler en az **44px** yüksekliğinde.

### 6.4 Chip / etiket
- Teknoloji chip'i: `background: var(--surface-2); border-radius: 10px; padding: 6px 11px; font-size: 13px; font-weight: 600`.
- Turuncu karo içindeki chip: `background: rgba(26,13,3,.1); border-radius: 999px`.
- Durum rozeti ("Yeni projelere açık"): `--success-tint` zemin, `--success-ink` yazı, 8px yeşil nokta.

### 6.5 Segment filtre (Projeler)
- Kapsayıcı: `background: var(--surface-2); padding: 5px; border-radius: 16px`.
- Seçili segment: beyaz zemin + `0 2px 8px rgba(17,19,23,.08)` gölge. Her segmentte sayı (`Web 9`).
- `<button aria-pressed="true|false">` kullan.

### 6.6 Proje kartı
- Beyaz karo, `padding: 12px`, üstte **150px kapak**: kategori tint zemini + proje monogramı (2 harf, 76px, 800, kategori ink rengi) + sol üstte beyaz kategori rozeti.
- Hover'da monogram `scale(1.08) rotate(-3deg)`.
- Altında: mono tarih + `↗`, başlık (19px/800), 1–2 satır açıklama, en altta mono tag listesi (`·` ile ayrılmış, en fazla 4).
- Ekran görüntüsü eklendiğinde monogram yerine `object-fit: cover` görsel kullanılır.

### 6.7 Zaman çizelgesi (Deneyim)
- 3 kolon: `150px tarih | 14px nokta+çizgi | içerik`.
- Güncel iş: dolu turuncu nokta, tarih `--accent-ink`. Geçmiş: 2px gri halka.
- Mobilde tarih kolonu gizlenir, tarih içerik başlığının üstüne mono etiket olarak gelir.

### 6.8 GitHub katkı grafiği
- 7 satır × 30 kolon kare, `gap: 4px`, kare `border-radius: 3px`, renkler `--heat-0..4`.
- Veri gerçek kaynaktan gelir (bkz. Aşama 7); veri gelmezse karo gizlenmez, "veri alınamadı" yazan sade boş durum gösterilir.

### 6.9 Form
- Input/textarea: `background: var(--surface-2); border:1px solid transparent; border-radius:14px; padding:14px 16px; font-size:16px`.
- Focus: `border-color: var(--accent); background:#fff; box-shadow: 0 0 0 4px rgba(249,115,22,.15)`.
- Her input'un görünür bir `<label>`'ı vardır. Placeholder etiketin yerine geçmez.

### 6.10 Footer
- **Statik** (asla `position: fixed`), içerik bittikten sonra gelir.
- Tek satır: sol `© {yıl} Ömer Faruk Türegün`, sağ mono `ASP.NET Core · Docker · Nginx`. `font-size:13px; color: var(--muted)`.
- Sosyal linkler footer'da değil, ana sayfadaki "linkler" karosunda ve İletişim sayfasında.

---

## 7. İkonlar

- **Bootstrap Icons** (mevcut) kullanılmaya devam eder; `stroke` tarzına yakın olanları seç.
- Emoji kullanılmaz.
- Yalnızca ikon içeren buton/link'te `aria-label` zorunlu; dekoratif ikonlarda `aria-hidden="true"`.

---

## 8. Hareket

| Efekt | Değer |
|---|---|
| Karo hover | `perspective(1000px) rotateX(3deg) translateY(-5px)`, `.35s cubic-bezier(.2,.7,.2,1)` |
| Kapak monogram hover | `scale(1.08) rotate(-3deg)`, `.5s` |
| Link hover (liste) | `padding-left: 6px`, `.2s` |
| Bölüm girişi (opsiyonel) | CSS `animation-timeline: view()` ile `opacity 0→1, translateY(24px→0)`; destek yoksa animasyonsuz görünür |

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after { animation: none !important; transition: none !important; }
}
```
- AOS kütüphanesi **kaldırılır**; JS'e bağlı giriş animasyonu yok.
- Hiçbir şey sürekli dönmez/nabız atmaz (tek istisna: durum noktası statik halka gölgesi).

---

## 9. Sayfa yapıları

Rota yapısı değişmez: `/{lang=tr|en}/Home/{Index|Projects|Experience|AboutMe|Contact}`.

### Ana sayfa (`Index`)
1. **Hero bento** (4 kolon):
   - `span-2 row-2` Tanıtım: 72px yuvarlak-kare fotoğraf + "Yeni projelere açık" rozeti, h1 "Selam, ben / Ömer Faruk.", lead paragraf, [Benimle çalış →] [CV indir].
   - Koyu karo "ŞU AN": şirket, unvan, alt satırda turuncu mono "önceden: …".
   - Konum karosu: nokta deseni + turuncu pin, "Kocaeli, TR", mono "UTC+3 · uzaktan / hibrit".
   - `span-2` turuncu "ÖNE ÇIKAN PROJE" karosu (projects.json'daki en yeni proje ya da `featured` olan).
   - `span-2` GitHub katkıları karosu.
   - Stack karosu (chip bulutu).
   - Linkler karosu (LinkedIn, GitHub, Medium, E-posta).
2. **Seçili işler**: başlık satırı + sağda "N projenin tümü →". Bento: 1 büyük (`span-2 row-2`, kapak alanlı) + 4 küçük proje karosu.
3. **Yolculuk**: 4 kolon deneyim karosu (güncel olan turuncu kenarlı).
4. **CTA**: koyu `--radius-2xl` karo, "Bir fikrin mi var? Birlikte inşa edelim." + turuncu buton.
5. Footer.

### Projeler (`Projects`)
1. Üst satır (`2fr 1fr 1fr`): başlık + açıklama + segment filtre | turuncu "TOPLAM" sayı karosu | koyu "FAVORİ ÜÇLÜ" karosu.
2. Proje kartı ızgarası (`auto-fill, minmax(280px,1fr)`), filtreye göre.
3. Karta tıklayınca mevcut detay modal'ı açılır (modal da bu sistemle yeniden stillenir: beyaz, `--radius-xl`, mono tarih, chip'ler).

### Deneyim (`Experience`)
1. `span-4` zaman çizelgesi karosu (iş deneyimleri + stajlar).
2. Eğitim karoları (`span-2` lisans, diğerleri tekli) + Dil karosu.
3. "SERTİFİKALAR" başlığı + 5 kolonluk sertifika karoları (mobilde 2 kolon).

### Hakkımda (`AboutMe`)
1. Bento: fotoğraf karosu (`row-2`, tam kaplayan görsel) | `span-2 row-2` tanıtım metni + CV butonu | turuncu "HEDEF" karosu | koyu sayılar karosu (şirket · proje · sertifika — **JSON'dan hesaplanır**, elle yazılmaz).
2. "KOD DIŞINDA": sosyal faaliyetler (about.json `socialActivities`) — fotoğraflı `span-2` karo, logo/koyu karo, "Boş zamanlarda" karosu.

### İletişim (`Contact`)
1. Bento: `span-2 row-3` form karosu (`tile--static`) — konu chip'leri, Ad, E-posta, Mesaj, [Gönder →]; gönderim sonrası yeşil başarı durumu.
2. Koyu `span-2` e-posta karosu: adres + [Adresi kopyala] (Clipboard API, "Kopyalandı ✓" geri bildirimi) + [Mail uygulamasında aç].
3. Konum karosu, turuncu "CV indir" karosu, `span-2` sosyal linkler karosu.
- Form gönderimi mevcut **Formspree** entegrasyonunu kullanmaya devam eder.

---

## 10. Dosya yapısı (güncel)

```
Views/
  Shared/
    _LayoutBento.cshtml        # ortak layout (SEO, fontlar, nav, footer, defer bento.js)
    _BentoHeader.cshtml        # pill nav + dil değiştirici
    _BentoFooter.cshtml        # statik footer
    _BentoLocationTile.cshtml  # konum karosu (Index + Contact)
  Home/
    Index*.cshtml, Projects*.cshtml, Experience*.cshtml,
    AboutMe*.cshtml, Contact*.cshtml, Status.cshtml, Privacy.cshtml
    Error → Views/Shared/Error.cshtml
wwwroot/
  css/bento.css                # token'lar + tüm bileşenler
  js/bento.js                  # Bento.* API (aşağıda)
  robots.txt, sitemap.xml
```

### `window.Bento` API (özet)
- `getLang`, `fetchJson`, `escapeHtml`, `copyToClipboard`, `sanitizeHtml`
- `projectCategory`, `monogram`, `yearOf`, `shortRange`
- `dialog.open(el, triggerEl)` / `close` / `bind` — native `<dialog>`; Projeler, sertifika, hedef, galeri
- Sayfa init: `home`, `projects`, `experience`, `about`, `contact`

### Dialog & galeri
- Proje detayı ve sertifika: beyaz `project-dialog`, backdrop blur, Esc/X/backdrop kapatır, odak tetikleyiciye döner.
- Hakkımda galeri: koyu sahne + thumbnail şeridi + yan (mobilde alt) sanitize edilmiş açıklama; ←/→ dialog üzerinde; yalnızca aktif + sonraki görsel önyüklenir.

**Kurallar**
- Sayfa içinde `<style>` bloğu yazılmaz; her stil `bento.css`'e gider.
- `!important` yalnızca `prefers-reduced-motion` bloğunda kullanılabilir.
- Her metin TR ve EN view'larda birlikte güncellenir (AGENTS.md kuralı).
- İçerik (`wwwroot/data`, `wwwroot/data-en`) elle değiştirilmez; JSON şeması korunur.
- Eski dark/glass dosyalar (`site-mobile.css`, `site-footer.css`, `_SiteFooter.cshtml`, `_Layout.cshtml`) kaldırıldı; public site yalnızca Bento.

---

## 11. Erişilebilirlik kontrol listesi

- [x] Metin kontrastı ≥ 4.5:1 (`--muted` ve `--accent-ink` beyaz/`--bg` üzerinde doğrulandı).
- [x] Tüm etkileşimli öğeler gerçek `<a href>` / `<button>`; div'e `onclick` yok.
- [x] Klavye ile menü, filtre, form ve dialog kullanılabilir; focus halkası `outline: 2px solid var(--accent-ink)`.
- [x] `lang` özniteliği TR/EN; hreflang + canonical layout'ta.
- [x] Fotoğraflarda anlamlı `alt`; dekoratiflerde `alt=""`.
- [x] 375px genişlikte yatay kaydırma yok (bento kırılımları).
- [x] `prefers-reduced-motion` açıkken transition/animation yok.
