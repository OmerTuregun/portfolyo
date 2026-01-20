using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;

namespace My_Portfolyo.Controllers
{
    public class AdminController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AuthService authService, ILogger<AdminController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: {lang}/Admin/Login
        [HttpGet]
        public IActionResult Login(string lang, string? returnUrl = null)
        {
            // Zaten giriş yapmışsa dashboard'a yönlendir
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

        // POST: {lang}/Admin/Login
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

            // Kimlik doğrulama
            if (_authService.ValidateCredentials(model.Username, model.Password))
            {
                // Session'a admin bilgisini kaydet
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUsername", model.Username);

                _logger.LogInformation($"Admin girişi başarılı: {model.Username}");

                // ReturnUrl varsa oraya, yoksa dashboard'a yönlendir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Dashboard", new { lang });
            }

            // Hatalı giriş
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

        // GET: {lang}/Admin/Dashboard (lang artık sadece URL için, panel tek)
        [AdminAuthorize]
        [HttpGet]
        public IActionResult Dashboard(string lang)
        {
            ViewData["Username"] = HttpContext.Session.GetString("AdminUsername") ?? "Admin";
            // Tek panel kullanacağız, lang sadece URL için
            return View("Dashboard");
        }

        // POST: {lang}/Admin/Logout
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
