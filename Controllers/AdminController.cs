using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;

namespace My_Portfolyo.Controllers
{
    public class AdminController : Controller
    {
        private readonly AuthService _authService;
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AuthService authService, JsonFileService jsonService, ILogger<AdminController> logger)
        {
            _authService = authService;
            _jsonService = jsonService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string lang, string? returnUrl = null)
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                return RedirectToAction("Dashboard", new { lang });
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Lang"] = lang ?? "tr";

            if (lang == "en")
            {
                return View("Login.en");
            }
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string lang, LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Lang"] = lang ?? "tr";
                if (lang == "en")
                {
                    return View("Login.en", model);
                }
                return View("Login", model);
            }

            if (_authService.ValidateCredentials(model.Username, model.Password))
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUsername", model.Username);

                _logger.LogInformation($"Admin girişi başarılı: {model.Username}");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Dashboard", new { lang });
            }

            ModelState.AddModelError("", lang == "en"
                ? "Invalid username or password."
                : "Geçersiz kullanıcı adı veya şifre.");

            ViewData["Lang"] = lang ?? "tr";
            if (lang == "en")
            {
                return View("Login.en", model);
            }
            return View("Login", model);
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard(string lang)
        {
            var contentLang = Request.Query["contentLang"].ToString().ToLower();
            if (string.IsNullOrEmpty(contentLang))
            {
                contentLang = Request.Query["lang"].ToString().ToLower();
            }
            if (contentLang != "tr" && contentLang != "en")
            {
                contentLang = "tr";
            }

            var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", contentLang);
            var certificates = await _jsonService.ReadJsonArrayAsync<CertificateViewModel>("certificates.json", contentLang);
            var references = await _jsonService.ReadJsonArrayAsync<ReferenceViewModel>("references.json", contentLang);
            var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", contentLang);

            var experienceItemCount = sections.Sum(s => (s.Items?.Count ?? 0) + (s.Experience?.Count ?? 0));

            ViewData["Username"] = HttpContext.Session.GetString("AdminUsername") ?? "Admin";
            ViewData["ProjectCount"] = projects.Count;
            ViewData["ExperienceCount"] = experienceItemCount;
            ViewData["CertificateCount"] = certificates.Count;
            ViewData["ReferenceCount"] = references.Count;
            ViewData["ContentLang"] = contentLang;

            return View("Dashboard");
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout(string lang)
        {
            HttpContext.Session.Clear();
            _logger.LogInformation("Admin çıkışı yapıldı");

            return RedirectToAction("Login", new { lang });
        }
    }
}
