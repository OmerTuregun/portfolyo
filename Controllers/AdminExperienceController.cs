using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Models.Admin;
using My_Portfolyo.Services;
using My_Portfolyo.Attributes;
using System.Text.Json;

namespace My_Portfolyo.Controllers
{
    [AdminAuthorize]
    public class AdminExperienceController : Controller
    {
        private readonly JsonFileService _jsonService;
        private readonly ILogger<AdminExperienceController> _logger;

        public AdminExperienceController(JsonFileService jsonService, ILogger<AdminExperienceController> logger)
        {
            _jsonService = jsonService;
            _logger = logger;
        }

        // GET: {lang}/AdminExperience?lang=tr&type=Eğitim
        public async Task<IActionResult> Index(string lang, string? contentLang = null, string? type = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
            
            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            ViewData["SelectedType"] = type;
            ViewData["Sections"] = sections;
            
            // Explicit view path because views are under Views/Admin/Experience
            return View("~/Views/Admin/Experience/Index.cshtml", sections);
        }

        // GET: {lang}/AdminExperience/Create?lang=tr&type=Eğitim
        public IActionResult Create(string lang, string? contentLang = null, string? type = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            ViewData["Type"] = type ?? "Eğitim";
            ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
            
            // Explicit view path
            return View("~/Views/Admin/Experience/Create.cshtml", new ExperienceViewModel());
        }

        // POST: {lang}/Admin/Experience/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string lang, ExperienceViewModel model, string? contentLang = null, string? type = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var experienceType = type ?? Request.Form["Type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                ViewData["Type"] = experienceType;
                ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
                // Explicit view path
                return View("~/Views/Admin/Experience/Create.cshtml", model);
            }

            try
            {
                var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
                
                // İlgili section'ı bul veya oluştur
                var section = sections.FirstOrDefault(s => s.Type == experienceType);
                if (section == null)
                {
                    section = new ExperienceSectionViewModel { Type = experienceType };
                    sections.Add(section);
                }

                // Yeni ID oluştur
                var allItems = new List<ExperienceViewModel>();
                if (section.Items != null) allItems.AddRange(section.Items);
                if (section.Experience != null) allItems.AddRange(section.Experience);
                
                var newId = allItems.Any() ? allItems.Max(e => e.Id) + 1 : 1;
                model.Id = newId;

                // Tags'i parse et (Diller hariç)
                if (experienceType != "Diller" && !string.IsNullOrEmpty(model.TagsInput))
                {
                    model.Tags = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }

                // Diller için özel alanlar
                if (experienceType == "Diller")
                {
                    if (int.TryParse(Request.Form["Percentage"].ToString(), out int percentage))
                    {
                        model.Percentage = percentage;
                    }
                    model.FlagCode = Request.Form["FlagCode"].ToString();
                    model.Tags = null; // Diller için tags yok
                }

                // Section tipine göre ekle
                if (experienceType == "Eğitim" || experienceType == "Diller")
                {
                    section.Items ??= new List<ExperienceViewModel>();
                    section.Items.Add(model);
                }
                else
                {
                    section.Experience ??= new List<ExperienceViewModel>();
                    section.Experience.Add(model);
                }

                await _jsonService.WriteJsonArrayAsync("experience.json", sections, currentLang);

                _logger.LogInformation($"Yeni deneyim eklendi: {model.Title} ({currentLang}, {experienceType})");

                return RedirectToAction("Index", new { lang, contentLang = currentLang, type = experienceType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deneyim ekleme hatası");
                ModelState.AddModelError("", "Deneyim eklenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                ViewData["Type"] = experienceType;
                ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
                return View(model);
            }
        }

        // GET: {lang}/AdminExperience/Edit/1?lang=tr&type=Eğitim
        public async Task<IActionResult> Edit(string lang, int id, string? contentLang = null, string? type = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var experienceType = type ?? Request.Query["type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
            var section = sections.FirstOrDefault(s => s.Type == experienceType);
            
            if (section == null)
            {
                return NotFound();
            }

            ExperienceViewModel? experience = null;
            if (section.Items != null)
            {
                experience = section.Items.FirstOrDefault(e => e.Id == id);
            }
            if (experience == null && section.Experience != null)
            {
                experience = section.Experience.FirstOrDefault(e => e.Id == id);
            }

            if (experience == null)
            {
                return NotFound();
            }

            // Tags'i string'e çevir
            experience.TagsInput = experience.Tags != null ? string.Join(", ", experience.Tags) : string.Empty;

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            ViewData["Type"] = experienceType;
            ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
            
            // Explicit view path
            return View("~/Views/Admin/Experience/Edit.cshtml", experience);
        }

        // POST: {lang}/Admin/Experience/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string lang, int id, ExperienceViewModel model, string? contentLang = null, string? type = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var experienceType = type ?? Request.Form["Type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            if (!ModelState.IsValid)
            {
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                ViewData["Type"] = experienceType;
                ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
                // Explicit view path
                return View("~/Views/Admin/Experience/Edit.cshtml", model);
            }

            try
            {
                var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
                var section = sections.FirstOrDefault(s => s.Type == experienceType);
                
                if (section == null)
                {
                    return NotFound();
                }

                model.Id = id;

                // Tags'i parse et (Diller hariç)
                if (experienceType != "Diller" && !string.IsNullOrEmpty(model.TagsInput))
                {
                    model.Tags = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }

                // Diller için özel alanlar
                if (experienceType == "Diller")
                {
                    if (int.TryParse(Request.Form["Percentage"].ToString(), out int percentage))
                    {
                        model.Percentage = percentage;
                    }
                    model.FlagCode = Request.Form["FlagCode"].ToString();
                    model.Tags = null; // Diller için tags yok
                }

                // Section tipine göre güncelle
                if (experienceType == "Eğitim" || experienceType == "Diller")
                {
                    if (section.Items == null) section.Items = new List<ExperienceViewModel>();
                    var itemIndex = section.Items.FindIndex(e => e.Id == id);
                    if (itemIndex == -1) return NotFound();
                    section.Items[itemIndex] = model;
                }
                else
                {
                    if (section.Experience == null) section.Experience = new List<ExperienceViewModel>();
                    var expIndex = section.Experience.FindIndex(e => e.Id == id);
                    if (expIndex == -1) return NotFound();
                    section.Experience[expIndex] = model;
                }

                await _jsonService.WriteJsonArrayAsync("experience.json", sections, currentLang);

                _logger.LogInformation($"Deneyim güncellendi: {model.Title} ({currentLang}, {experienceType})");

                return RedirectToAction("Index", new { lang, contentLang = currentLang, type = experienceType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deneyim güncelleme hatası");
                ModelState.AddModelError("", "Deneyim güncellenirken bir hata oluştu.");
                ViewData["CurrentLang"] = currentLang;
                ViewData["Lang"] = lang ?? "tr";
                ViewData["Type"] = experienceType;
                ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
                return View(model);
            }
        }

        // POST: {lang}/Admin/Experience/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string lang, int id, string? contentLang = null, string? type = null)
        {
            var currentLang = contentLang ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            var experienceType = type ?? Request.Query["type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            try
            {
                var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
                var section = sections.FirstOrDefault(s => s.Type == experienceType);
                
                if (section == null)
                {
                    return NotFound();
                }

                ExperienceViewModel? experience = null;
                bool removed = false;

                if (section.Items != null)
                {
                    experience = section.Items.FirstOrDefault(e => e.Id == id);
                    if (experience != null)
                    {
                        section.Items.Remove(experience);
                        removed = true;
                    }
                }
                
                if (!removed && section.Experience != null)
                {
                    experience = section.Experience.FirstOrDefault(e => e.Id == id);
                    if (experience != null)
                    {
                        section.Experience.Remove(experience);
                        removed = true;
                    }
                }

                if (!removed || experience == null)
                {
                    return NotFound();
                }

                await _jsonService.WriteJsonArrayAsync("experience.json", sections, currentLang);

                _logger.LogInformation($"Deneyim silindi: {experience.Title} ({currentLang}, {experienceType})");

                return RedirectToAction("Index", new { lang, contentLang = currentLang, type = experienceType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deneyim silme hatası");
                return RedirectToAction("Index", new { lang, contentLang = currentLang, type = experienceType });
            }
        }
    }
}
