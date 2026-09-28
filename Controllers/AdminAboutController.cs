using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;

namespace My_Portfolyo.Controllers
{
    [AdminAuthorize]
    public class AdminAboutController : Controller
    {
        private const string ViewRoot = "~/Views/Admin/About/";
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminAboutController> _logger;

        public AdminAboutController(JsonFileService jsonService, ILogger<AdminAboutController> logger)
        {
            _jsonService = jsonService;
            _logger = logger;
        }

        private string ResolveContentLang(string? contentLang, string? lang)
        {
            string? current = contentLang;
            if (string.IsNullOrEmpty(current) && Request.HasFormContentType)
            {
                current = Request.Form["contentLang"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(current))
            {
                current = Request.Query["contentLang"].FirstOrDefault() ?? lang ?? "tr";
            }
            current = current.ToLowerInvariant();
            return (current == "tr" || current == "en") ? current : "tr";
        }

        public async Task<IActionResult> Index(string lang, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            var about = await _jsonService.ReadJsonFileAsync<AboutViewModel>("about.json", currentLang)
                        ?? new AboutViewModel();

            foreach (var activity in about.SocialActivities)
            {
                activity.CarouselImagesInput = string.Join("\n", activity.CarouselImages);
                activity.DescriptionInput = string.Join("\n", activity.Description);
            }

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Index.cshtml", about);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string lang, AboutViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);

            // Form'dan gelen sosyal aktiviteleri parse et
            model.SocialActivities = ParseSocialActivitiesFromForm();

            if (string.IsNullOrWhiteSpace(model.AboutMeText) || string.IsNullOrWhiteSpace(model.FutureGoalText))
            {
                ModelState.AddModelError("", currentLang == "tr"
                    ? "Hakkımda ve gelecek hedefi alanları zorunludur."
                    : "About and future goal fields are required.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Index.cshtml", model);
            }

            try
            {
                await _jsonService.WriteJsonFileAsync("about.json", model, currentLang);
                _logger.LogInformation($"Hakkımda güncellendi ({currentLang})");
                TempData["Success"] = currentLang == "tr" ? "Hakkımda başarıyla kaydedildi." : "About saved successfully.";
                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hakkımda kaydetme hatası");
                ModelState.AddModelError("", "Kaydetme sırasında bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Index.cshtml", model);
            }
        }

        private List<SocialActivityViewModel> ParseSocialActivitiesFromForm()
        {
            var result = new List<SocialActivityViewModel>();
            var ids = Request.Form["activityId"];
            var titles = Request.Form["activityTitle"];
            var images = Request.Form["activityImages"];
            var descriptions = Request.Form["activityDescription"];

            for (var i = 0; i < titles.Count; i++)
            {
                var title = titles[i]?.Trim() ?? "";
                if (string.IsNullOrEmpty(title)) continue;

                var id = i < ids.Count ? ids[i]?.Trim() : null;
                if (string.IsNullOrEmpty(id))
                {
                    id = title.ToLowerInvariant()
                        .Replace(" ", "-")
                        .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
                        .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                }

                var imagesRaw = i < images.Count ? images[i] ?? "" : "";
                var descRaw = i < descriptions.Count ? descriptions[i] ?? "" : "";

                result.Add(new SocialActivityViewModel
                {
                    Id = id!,
                    Title = title,
                    CarouselImages = SplitImages(imagesRaw),
                    Description = SplitLines(descRaw),
                    CarouselImagesInput = imagesRaw,
                    DescriptionInput = descRaw
                });
            }

            return result;
        }

        private static List<string> SplitImages(string raw)
        {
            return raw
                .Replace("\r\n", "\n")
                .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        private static List<string> SplitLines(string raw)
        {
            return raw
                .Replace("\r\n", "\n")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
    }
}
