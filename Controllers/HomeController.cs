using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models;

namespace My_Portfolyo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string lang)
        {
            if (lang == "en")
            {
                return View("Index.en");
            }
            return View("Index");
        }

        public IActionResult Projects(string lang)
        {
            if (lang == "en")
            {
                return View("Projects.en");
            }
            return View("Projects");
        }

        public IActionResult Contact(string lang)
        {
            if (lang == "en")
            {
                return View("Contact.en");
            }
            return View("Contact");
        }

        public IActionResult Experience(string lang)
        {
            if (lang == "en")
            {
                return View("Experience.en");
            }
            return View("Experience");
        }

        public IActionResult AboutMe(string lang)
        {
            if (lang == "en")
            {
                return View("AboutMe.en");
            }
            return View("AboutMe");
        }

        public IActionResult Privacy(string lang)
        {
            ViewData["Title"] = lang == "en" ? "Privacy" : "Gizlilik";
            ViewData["Description"] = lang == "en"
                ? "Privacy notes for omerfarukturegun.com.tr"
                : "omerfarukturegun.com.tr gizlilik notları";
            return View();
        }

        public IActionResult Status(string lang, int id)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var originalPath = feature?.OriginalPath ?? string.Empty;
            var isEn = originalPath.StartsWith("/en", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);

            var code = id < 400 ? 404 : id;

            Response.StatusCode = code;
            ViewData["Title"] = code.ToString();
            ViewData["StatusCode"] = code;
            ViewData["IsEn"] = isEn;
            ViewData["Description"] = isEn
                ? "The requested page could not be found or an error occurred."
                : "İstenen sayfa bulunamadı veya bir hata oluştu.";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string lang)
        {
            var isEn = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);
            ViewData["Title"] = isEn ? "Error" : "Hata";
            ViewData["IsEn"] = isEn;
            ViewData["Description"] = isEn
                ? "Something went wrong while processing your request."
                : "İsteğiniz işlenirken bir sorun oluştu.";

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
