using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;

namespace My_Portfolyo.Controllers
{
    [AdminAuthorize]
    public class AdminReferencesController : Controller
    {
        private const string ViewRoot = "~/Views/Admin/References/";
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminReferencesController> _logger;

        public AdminReferencesController(JsonFileService jsonService, ILogger<AdminReferencesController> logger)
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
            var items = await _jsonService.ReadJsonArrayAsync<ReferenceViewModel>("references.json", currentLang);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Index.cshtml", items);
        }

        public IActionResult Create(string lang, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Create.cshtml", new ReferenceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string lang, ReferenceViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Create.cshtml", model);
            }

            try
            {
                var items = await _jsonService.ReadJsonArrayAsync<ReferenceViewModel>("references.json", currentLang);
                model.Id = items.Any() ? items.Max(r => r.Id) + 1 : 1;
                items.Add(model);
                await _jsonService.WriteJsonArrayAsync("references.json", items, currentLang);
                _logger.LogInformation($"Referans eklendi: {model.Name} ({currentLang})");
                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans ekleme hatası");
                ModelState.AddModelError("", "Referans eklenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Create.cshtml", model);
            }
        }

        public async Task<IActionResult> Edit(string lang, int id, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            var items = await _jsonService.ReadJsonArrayAsync<ReferenceViewModel>("references.json", currentLang);
            var item = items.FirstOrDefault(r => r.Id == id);
            if (item == null) return NotFound();

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            return View(ViewRoot + "Edit.cshtml", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string lang, int id, ReferenceViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ViewRoot + "Edit.cshtml", model);
            }

            try
            {
                var items = await _jsonService.ReadJsonArrayAsync<ReferenceViewModel>("references.json", currentLang);
                var index = items.FindIndex(r => r.Id == id);
                if (index == -1) return NotFound();

                model.Id = id;
                items[index] = model;
                await _jsonService.WriteJsonArrayAsync("references.json", items, currentLang);
                _logger.LogInformation($"Referans güncellendi: {model.Name} ({currentLang})");
                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans güncelleme hatası");
                ModelState.AddModelError("", "Referans güncellenirken bir hata oluştu.");
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
                var items = await _jsonService.ReadJsonArrayAsync<ReferenceViewModel>("references.json", currentLang);
                var item = items.FirstOrDefault(r => r.Id == id);
                if (item == null) return NotFound();
                items.Remove(item);
                await _jsonService.WriteJsonArrayAsync("references.json", items, currentLang);
                _logger.LogInformation($"Referans silindi: {item.Name} ({currentLang})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans silme hatası");
            }
            return RedirectToAction("Index", new { lang, contentLang = currentLang });
        }
    }
}
