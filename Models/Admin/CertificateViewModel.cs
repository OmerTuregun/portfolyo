using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace My_Portfolyo.Models.Admin
{
    public class CertificateViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir")]
        [Display(Name = "Başlık")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurum gereklidir")]
        [Display(Name = "Kurum")]
        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tarih gereklidir")]
        [Display(Name = "Tarih")]
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [Display(Name = "Beceriler (virgülle ayırın)")]
        [JsonIgnore]
        public string SkillsInput { get; set; } = string.Empty;

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = new();

        [Display(Name = "Görsel URL")]
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
