using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;
using System.Text.Json;

namespace My_Portfolyo.Controllers
{
    [AdminAuthorize]
    public class AdminProjectsController : Controller
    {
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminProjectsController> _logger;

        public AdminProjectsController(JsonFileService jsonService, ILogger<AdminProjectsController> logger)
        {
            _jsonService = jsonService;
            _logger = logger;
        }

        // GET: {lang}/Admin/Projects?lang=tr
        public async Task<IActionResult> Index(string lang, string? contentLang = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
            
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            
            return View(projects);
        }

        // GET: {lang}/Admin/Projects/Create?lang=tr
        public IActionResult Create(string lang, string? contentLang = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            
            return View(new ProjectViewModel());
        }

        // POST: {lang}/Admin/Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string lang, ProjectViewModel model, string? contentLang = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(model);
            }

            try
            {
                // Mevcut projeleri oku
                var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
                
                // Yeni ID oluştur
                var newId = projects.Any() ? projects.Max(p => p.Id) + 1 : 1;
                model.Id = newId;

                // Tags'i parse et
                if (!string.IsNullOrEmpty(model.TagsInput))
                {
                    model.Tags = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }

                // ModalContent'i parse et (HTML olarak)
                if (!string.IsNullOrEmpty(Request.Form["ModalContent"]))
                {
                    var modalContent = Request.Form["ModalContent"].ToString();
                    model.ModalContent = modalContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToList();
                }

                projects.Add(model);

                // JSON'a yaz
                await _jsonService.WriteJsonArrayAsync("projects.json", projects, currentLang);

                _logger.LogInformation($"Yeni proje eklendi: {model.Title} ({currentLang})");

                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje ekleme hatası");
                ModelState.AddModelError("", "Proje eklenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(model);
            }
        }

        // GET: {lang}/Admin/Projects/Edit/1?lang=tr
        public async Task<IActionResult> Edit(string lang, int id, string? contentLang = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
            var project = projects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Tags'i string'e çevir
            project.TagsInput = string.Join(", ", project.Tags);

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            
            return View(project);
        }

        // POST: {lang}/Admin/Projects/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string lang, int id, ProjectViewModel model, string? contentLang = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(model);
            }

            try
            {
                var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
                var projectIndex = projects.FindIndex(p => p.Id == id);

                if (projectIndex == -1)
                {
                    return NotFound();
                }

                model.Id = id;

                // Tags'i parse et
                if (!string.IsNullOrEmpty(model.TagsInput))
                {
                    model.Tags = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }

                // ModalContent'i parse et
                if (!string.IsNullOrEmpty(Request.Form["ModalContent"]))
                {
                    var modalContent = Request.Form["ModalContent"].ToString();
                    model.ModalContent = modalContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToList();
                }

                projects[projectIndex] = model;

                await _jsonService.WriteJsonArrayAsync("projects.json", projects, currentLang);

                _logger.LogInformation($"Proje güncellendi: {model.Title} ({currentLang})");

                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje güncelleme hatası");
                ModelState.AddModelError("", "Proje güncellenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(model);
            }
        }

        // POST: {lang}/Admin/Projects/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string lang, int id, string? contentLang = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            try
            {
                var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
                var project = projects.FirstOrDefault(p => p.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                projects.Remove(project);
                await _jsonService.WriteJsonArrayAsync("projects.json", projects, currentLang);

                _logger.LogInformation($"Proje silindi: {project.Title} ({currentLang})");

                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje silme hatası");
                return RedirectToAction("Index", new { lang, contentLang = currentLang });
            }
        }
    }
}
