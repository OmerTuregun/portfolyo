using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace My_Portfolyo.Models.Admin
{
    public class ExperienceViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir")]
        [Display(Name = "Başlık")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Şirket/Kurum")]
        [JsonPropertyName("company")]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Tarih aralığı gereklidir")]
        [Display(Name = "Tarih Aralığı")]
        [JsonPropertyName("dateRange")]
        public string DateRange { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [Display(Name = "Etiketler (virgülle ayırın)")]
        [JsonIgnore]
        public string TagsInput { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        // Diller için özel alanlar
        [Display(Name = "Yüzde")]
        [JsonPropertyName("percentage")]
        public int? Percentage { get; set; }

        [Display(Name = "Bayrak Kodu")]
        [JsonPropertyName("flagCode")]
        public string? FlagCode { get; set; }
    }

    // Experience JSON yapısı için wrapper
    public class ExperienceSectionViewModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<ExperienceViewModel>? Items { get; set; }

        [JsonPropertyName("experience")]
        public List<ExperienceViewModel>? Experience { get; set; }
    }
}
