using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace My_Portfolyo.Models.Admin
{
    public class ReferenceViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim gereklidir")]
        [Display(Name = "İsim")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ünvan gereklidir")]
        [Display(Name = "Ünvan")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şirket gereklidir")]
        [Display(Name = "Şirket")]
        [JsonPropertyName("company")]
        public string Company { get; set; } = string.Empty;
    }
}
