using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// .env dosyasını yükle (container içinde /src/.env konumunda olmalı)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($".env dosyası yüklendi: {envPath}");
}
else
{
    Console.WriteLine($"UYARI: .env dosyası bulunamadı: {envPath}");
    // Alternatif konumları dene
    var altEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
    if (File.Exists(altEnvPath))
    {
        Env.Load(altEnvPath);
        Console.WriteLine($".env dosyası alternatif konumdan yüklendi: {altEnvPath}");
    }
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Custom Services
builder.Services.AddScoped<My_Portfolyo.Services.JsonFileService>();
builder.Services.AddScoped<My_Portfolyo.Services.AuthService>();

// Session yapılandırması
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'i ekle (UseRouting'den sonra, UseAuthorization'den önce)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default_lang",
    pattern: "{lang=tr}/{controller=Home}/{action=Index}/{id?}", // Başına {lang=tr} ekledik
    constraints: new { lang = "tr|en" }); // Sadece 'tr' veya 'en' kabul et

app.Run();
