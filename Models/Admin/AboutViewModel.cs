using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace My_Portfolyo.Models.Admin
{
    public class AboutViewModel
    {
        [Required(ErrorMessage = "Hakkımda metni gereklidir")]
        [Display(Name = "Hakkımda")]
        [JsonPropertyName("aboutMeText")]
        public string AboutMeText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hedef metni gereklidir")]
        [Display(Name = "Gelecek Hedefi")]
        [JsonPropertyName("futureGoalText")]
        public string FutureGoalText { get; set; } = string.Empty;

        [JsonPropertyName("socialActivities")]
        public List<SocialActivityViewModel> SocialActivities { get; set; } = new();
    }

    public class SocialActivityViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlık gereklidir")]
        [Display(Name = "Başlık")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Görseller (virgülle veya satır satır)")]
        [JsonIgnore]
        public string CarouselImagesInput { get; set; } = string.Empty;

        [JsonPropertyName("carouselImages")]
        public List<string> CarouselImages { get; set; } = new();

        [Display(Name = "Açıklama (HTML, satır satır)")]
        [JsonIgnore]
        public string DescriptionInput { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public List<string> Description { get; set; } = new();
    }
}
