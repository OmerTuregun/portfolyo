using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;

namespace My_Portfolyo.Controllers
{
    [AdminAuthorize]
    public class AdminProjectsController : Controller
    {
        private const string ProjectsView = "~/Views/Admin/Projects/";
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminProjectsController> _logger;

        public AdminProjectsController(JsonFileService jsonService, ILogger<AdminProjectsController> logger)
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
            var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";

            return View(ProjectsView + "Index.cshtml", projects);
        }

        public IActionResult Create(string lang, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";

            return View(ProjectsView + "Create.cshtml", new ProjectViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string lang, ProjectViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                return View(ProjectsView + "Create.cshtml", model);
            }

            try
            {
                var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
                model.Id = projects.Any() ? projects.Max(p => p.Id) + 1 : 1;
                ParseTagsAndModal(model);

                projects.Add(model);
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
                return View(ProjectsView + "Create.cshtml", model);
            }
        }

        public async Task<IActionResult> Edit(string lang, int id, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);
            var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
            var project = projects.FirstOrDefault(p => p.Id == id);

            if (project == null) return NotFound();

            project.TagsInput = string.Join(", ", project.Tags);
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";

            return View(ProjectsView + "Edit.cshtml", project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string lang, int id, ProjectViewModel model, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);

            try
            {
                var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
                var projectIndex = projects.FindIndex(p => p.Id == id);

                if (projectIndex == -1) return NotFound();

                if (projects.Any(p => p.Id == model.Id && p.Id != id))
                {
                    ModelState.AddModelError(nameof(model.Id), currentLang == "tr"
                        ? "Bu ID değerine sahip başka bir proje zaten var."
                        : "Another project with this ID already exists.");
                    ViewData["CurrentLang"] = currentLang;
                    ViewData["Lang"] = lang ?? "tr";
                    return View(ProjectsView + "Edit.cshtml", model);
                }

                if (!ModelState.IsValid)
                {
                    ViewData["CurrentLang"] = currentLang;
                    ViewData["Lang"] = lang ?? "tr";
                    return View(ProjectsView + "Edit.cshtml", model);
                }

                ParseTagsAndModal(model);
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
                return View(ProjectsView + "Edit.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string lang, int id, string? contentLang = null)
        {
            var currentLang = ResolveContentLang(contentLang, lang);

            try
            {
                var projects = await _jsonService.ReadJsonArrayAsync<ProjectViewModel>("projects.json", currentLang);
                var project = projects.FirstOrDefault(p => p.Id == id);
                if (project == null) return NotFound();

                projects.Remove(project);
                await _jsonService.WriteJsonArrayAsync("projects.json", projects, currentLang);
                _logger.LogInformation($"Proje silindi: {project.Title} ({currentLang})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje silme hatası");
            }

            return RedirectToAction("Index", new { lang, contentLang = currentLang });
        }

        private void ParseTagsAndModal(ProjectViewModel model)
        {
            if (!string.IsNullOrEmpty(model.TagsInput))
            {
                model.Tags = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(Request.Form["ModalContent"]))
            {
                var modalContent = Request.Form["ModalContent"].ToString();
                model.ModalContent = modalContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line))
                    .ToList();
            }
        }
    }
}
