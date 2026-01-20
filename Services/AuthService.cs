using BCrypt.Net;
using DotNetEnv;

namespace My_Portfolyo.Services
{
    public class AuthService
    {
        private readonly ILogger<AuthService> _logger;

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }

        public bool ValidateCredentials(string username, string password)
        {
            try
            {
                // .env dosyasından direkt oku (DotNetEnv ile yüklenmiş olmalı)
                var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
                var adminPasswordHash = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "";

                // Debug: Hash uzunluğunu logla
                _logger.LogInformation($"Hash uzunluğu: {adminPasswordHash.Length}, Hash başlangıcı: {adminPasswordHash.Substring(0, Math.Min(30, adminPasswordHash.Length))}");

                // Kullanıcı adı kontrolü
                if (username != adminUsername)
                {
                    _logger.LogWarning($"Geçersiz kullanıcı adı denemesi: {username}");
                    return false;
                }

                // Şifre hash kontrolü
                if (string.IsNullOrEmpty(adminPasswordHash))
                {
                    _logger.LogError("ADMIN_PASSWORD environment variable bulunamadı!");
                    return false;
                }

                // Eğer hash değilse (plain text), direkt karşılaştır (geçici çözüm)
                if (!adminPasswordHash.StartsWith("$2a$") && !adminPasswordHash.StartsWith("$2b$") && !adminPasswordHash.StartsWith("$2y$"))
                {
                    _logger.LogWarning("ADMIN_PASSWORD BCrypt hash değil, plain text olarak kontrol ediliyor (güvenlik riski!)");
                    // Plain text karşılaştırma (sadece development için)
                    bool isValid = password == adminPasswordHash;
                    if (isValid)
                    {
                        _logger.LogWarning("Plain text şifre kullanıldı! Lütfen .env dosyasına BCrypt hash ekleyin.");
                    }
                    return isValid;
                }

                // Hash uzunluğu kontrolü (BCrypt hash'i genellikle 60 karakter)
                if (adminPasswordHash.Length < 59)
                {
                    _logger.LogError($"Hash çok kısa! Uzunluk: {adminPasswordHash.Length}, Beklenen: 60");
                    return false;
                }

                // BCrypt ile şifre doğrulama
                try
                {
                    // BCrypt.Net-Next için hash'i temizle (başında/sonunda boşluk varsa)
                    adminPasswordHash = adminPasswordHash.Trim();
                    
                    // Hash formatını tekrar kontrol et
                    if (adminPasswordHash.Length != 60)
                    {
                        _logger.LogError($"Hash uzunluğu yanlış! Beklenen: 60, Mevcut: {adminPasswordHash.Length}");
                        _logger.LogError($"Hash içeriği: {adminPasswordHash}");
                        return false;
                    }

                    // BCrypt.Net-Next Verify metodunu kullan (basit versiyon)
                    // BCrypt.Net-Next 4.0.3 için doğru kullanım
                    bool isValid = BCrypt.Net.BCrypt.Verify(password, adminPasswordHash, false);
                    
                    if (!isValid)
                    {
                        _logger.LogWarning("Geçersiz şifre denemesi");
                    }

                    return isValid;
                }
                catch (IndexOutOfRangeException ex)
                {
                    _logger.LogError(ex, $"BCrypt hash formatı hatası. Hash uzunluğu: {adminPasswordHash.Length}");
                    _logger.LogError($"Hash içeriği (ilk 30 karakter): {adminPasswordHash.Substring(0, Math.Min(30, adminPasswordHash.Length))}");
                    // Hash muhtemelen bozuk, yeni hash oluşturulması gerekiyor
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"BCrypt doğrulama hatası. Hash uzunluğu: {adminPasswordHash.Length}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kimlik doğrulama hatası");
                return false;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }
    }
}
