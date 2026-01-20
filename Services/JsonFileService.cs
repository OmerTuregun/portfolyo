using System.Text.Json;
using System.Text.Json.Serialization;

namespace My_Portfolyo.Services
{
    public class JsonFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<JsonFileService> _logger;
        private static readonly object _lockObject = new();

        public JsonFileService(IWebHostEnvironment environment, ILogger<JsonFileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        private string GetDataPath(string fileName, string lang = "tr")
        {
            var folder = lang == "en" ? "data-en" : "data";
            return Path.Combine(_environment.WebRootPath, folder, fileName);
        }

        private void CreateBackup(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var backupPath = $"{filePath}.backup.{DateTime.Now:yyyyMMddHHmmss}";
                    File.Copy(filePath, backupPath);
                    _logger.LogInformation($"Backup oluşturuldu: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Backup oluşturulurken hata: {filePath}");
            }
        }

        public async Task<T?> ReadJsonFileAsync<T>(string fileName, string lang = "tr")
        {
            var filePath = GetDataPath(fileName, lang);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Dosya bulunamadı: {filePath}");
                return default(T);
            }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                
                return JsonSerializer.Deserialize<T>(jsonContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"JSON okuma hatası: {filePath}");
                throw;
            }
        }

        public Task WriteJsonFileAsync<T>(string fileName, T data, string lang = "tr")
        {
            var filePath = GetDataPath(fileName, lang);
            
            lock (_lockObject)
            {
                try
                {
                    // Backup oluştur
                    CreateBackup(filePath);

                    // Dizini oluştur (yoksa)
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // JSON yazma seçenekleri
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    var jsonContent = JsonSerializer.Serialize(data, options);
                    File.WriteAllText(filePath, jsonContent, System.Text.Encoding.UTF8);
                    
                    _logger.LogInformation($"JSON dosyası yazıldı: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"JSON yazma hatası: {filePath}");
                    throw;
                }
            }
            
            return Task.CompletedTask;
        }

        public async Task<List<T>> ReadJsonArrayAsync<T>(string fileName, string lang = "tr")
        {
            var result = await ReadJsonFileAsync<List<T>>(fileName, lang);
            return result ?? new List<T>();
        }

        public async Task WriteJsonArrayAsync<T>(string fileName, List<T> data, string lang = "tr")
        {
            await WriteJsonFileAsync(fileName, data, lang);
        }
    }
}
