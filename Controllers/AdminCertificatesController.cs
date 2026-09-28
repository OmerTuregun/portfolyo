using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;

namespace My_Portfolyo.Controllers
{
    [AdminAuthorize]
    public class AdminCertificatesController : Controller
    {
        private const string ViewRoot = "~/Views/Admin/Certificates/";
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminCertificatesController> _logger;

        public AdminCertificatesController(JsonFileService jsonService, ILogger<AdminCertificatesController> logger)
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
            var items = await _jsonService.ReadJsonArrayAsync<CertificateViewModel>("certificates.json", currentLang);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Index.cshtml", items);
        }

        public IActionResult Create(string lang, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Create.cshtml", new CertificateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string lang, CertificateViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            ParseSkills(model);

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Create.cshtml", model);
            }

            try
            {
                var items = await _jsonService.ReadJsonArrayAsync<CertificateViewModel>("certificates.json", currentLang);
                model.Id = items.Any() ? items.Max(c => c.Id) + 1 : 1;
                items.Add(model);
                await _jsonService.WriteJsonArrayAsync("certificates.json", items, currentLang);
                _logger.LogInformation($"Sertifika eklendi: {model.Title} ({currentLang})");
                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sertifika ekleme hatası");
                ModelState.AddModelError("", "Sertifika eklenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Create.cshtml", model);
            }
        }

        public async Task<IActionResult> Edit(string lang, int id, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            var items = await _jsonService.ReadJsonArrayAsync<CertificateViewModel>("certificates.json", currentLang);
            var item = items.FirstOrDefault(c => c.Id == id);
            if (item == null) return NotFound();

            item.SkillsInput = string.Join(", ", item.Skills);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Edit.cshtml", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string lang, int id, CertificateViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            ParseSkills(model);

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Edit.cshtml", model);
            }

            try
            {
                var items = await _jsonService.ReadJsonArrayAsync<CertificateViewModel>("certificates.json", currentLang);
                var index = items.FindIndex(c => c.Id == id);
                if (index == -1) return NotFound();

                model.Id = id;
                items[index] = model;
                await _jsonService.WriteJsonArrayAsync("certificates.json", items, currentLang);
                _logger.LogInformation($"Sertifika güncellendi: {model.Title} ({currentLang})");
                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sertifika güncelleme hatası");
                ModelState.AddModelError("", "Sertifika güncellenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Edit.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string lang, int id, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            try
            {
                var items = await _jsonService.ReadJsonArrayAsync<CertificateViewModel>("certificates.json", currentLang);
                var item = items.FirstOrDefault(c => c.Id == id);
                if (item == null) return NotFound();
                items.Remove(item);
                await _jsonService.WriteJsonArrayAsync("certificates.json", items, currentLang);
                _logger.LogInformation($"Sertifika silindi: {item.Title} ({currentLang})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sertifika silme hatası");
            }
            return RedirectToAction("Index", new { lang, contentLang = currentLang });
        }

        private static void ParseSkills(CertificateViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SkillsInput))
            {
                model.Skills = model.SkillsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
        }
    }
}
