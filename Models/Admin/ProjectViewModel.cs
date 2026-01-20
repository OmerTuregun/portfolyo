using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace My_Portfolyo.Models.Admin
{
    public class ProjectViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tarih aralığı gereklidir")]
        [Display(Name = "Tarih Aralığı")]
        [JsonPropertyName("dateRange")]
        public string DateRange { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlık gereklidir")]
        [Display(Name = "Başlık")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama gereklidir")]
        [Display(Name = "Açıklama")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Görsel URL")]
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "GitHub URL")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        [JsonPropertyName("githubUrl")]
        public string? GithubUrl { get; set; }

        [Display(Name = "Etiketler (virgülle ayırın)")]
        [JsonIgnore]
        public string TagsInput { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [Display(Name = "Modal İçeriği (HTML)")]
        [JsonPropertyName("modalContent")]
        public List<string> ModalContent { get; set; } = new();
    }
}
