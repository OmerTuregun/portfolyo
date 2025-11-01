using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models; // Bu senin namespace'in, aynı kalıyor

namespace My_Portfolyo.Controllers // Bu senin namespace'in, aynı kalıyor
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // --- GÜNCELLENDİ: "lang" parametresi eklendi ---
        // Plan B, Adım 1'de (Program.cs) tanımladığımız {lang} rotasından gelir
        public IActionResult Index(string lang)
        {
            // Eğer URL /en/Home/Index ise...
            if (lang == "en")
            {
                // Index.en.cshtml dosyasını göster
                return View("Index.en");
            }
            // Değilse (varsayılan 'tr' ise), normal Index.cshtml'i göster
            return View("Index");
        }

        // --- GÜNCELLENDİ: "lang" parametresi eklendi ---
        public IActionResult Projects(string lang)
        {
            if (lang == "en")
            {
                return View("Projects.en");
            }
            return View("Projects");
        }

        // --- GÜNCELLENDİ: "lang" parametresi eklendi ---
        public IActionResult Contact(string lang)
        {
            if (lang == "en")
            {
                return View("Contact.en");
            }
            return View("Contact");
        }

        // --- GÜNCELLENDİ: "lang" parametresi eklendi ---
        public IActionResult Experience(string lang)
        {
            if (lang == "en")
            {
                return View("Experience.en");
            }
            return View("Experience");
        }

        // --- GÜNCELLENDİ: "lang" parametresi eklendi ---
        public IActionResult AboutMe(string lang)
        {
            if (lang == "en")
            {
                return View("AboutMe.en");
            }
            return View("AboutMe");
        }


        // --- Bu standart metotlara dokunmadık ---
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}