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

        /// <summary>
        /// EN JSON dosyalarında type alanları İngilizce (Education, Work Experience, Internships, Languages).
        /// Admin arayüzü ise Türkçe type değerleri (Eğitim, İş Deneyimi, Stajlar, Diller) ile çalışıyor.
        /// Bu helper, okunan section.Type değerlerini TR karşılıklarına normalize eder ki
        /// filtreler ve UI her dilde tutarlı çalışsın.
        /// </summary>
        private void NormalizeSectionTypesForLanguage(List<ExperienceSectionViewModel> sections, string currentLang)
        {
            if (currentLang != "en") return;

            foreach (var section in sections)
            {
                section.Type = section.Type switch
                {
                    "Education" => "Eğitim",
                    "Work Experience" => "İş Deneyimi",
                    "WorkExperience" => "İş Deneyimi",
                    "Internships" => "Stajlar",
                    "Languages" => "Diller",
                    _ => section.Type
                };
            }
        }

        // GET: {lang}/AdminExperience?lang=tr&type=Eğitim
        public async Task<IActionResult> Index(string lang, string? contentLang = null, string? type = null)
        {
            // lang route parametresinden geliyor, contentLang query string'den
            // Eğer contentLang belirtilmemişse, route'daki lang'i kullan
            var currentLang = contentLang ?? Request.Query["contentLang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                // contentLang yoksa, route'daki lang'i kullan (veya default tr)
                currentLang = lang ?? "tr";
            }

            var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
            // EN tarafındaki type değerlerini TR karşılıklarına normalize et
            NormalizeSectionTypesForLanguage(sections, currentLang);
            
            ViewData["CurrentLang"] = currentLang;
            // lang her zaman route'dan geliyor, bu admin panel dilini belirler (TR/EN butonları için)
            ViewData["Lang"] = lang ?? "tr";
            ViewData["SelectedType"] = type;
            ViewData["Sections"] = sections;
            
            // Explicit view path because views are under Views/Admin/Experience
            return View("~/Views/Admin/Experience/Index.cshtml", sections);
        }

        // GET: {lang}/AdminExperience/Create?lang=tr&type=Eğitim
        public IActionResult Create(string lang, string? contentLang = null, string? type = null)
        {
            // contentLang önce route parametresinden, sonra query string'den al
            var currentLang = contentLang ?? Request.Query["contentLang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = lang ?? "tr";
            }

            ViewData["CurrentLang"] = currentLang;
            ViewData["Lang"] = lang ?? "tr";
            ViewData["Type"] = type ?? Request.Query["type"].ToString() ?? "Eğitim";
            ViewData["Types"] = new List<string> { "Eğitim", "İş Deneyimi", "Stajlar", "Diller" };
            
            // Explicit view path
            return View("~/Views/Admin/Experience/Create.cshtml", new ExperienceViewModel());
        }

        // POST: {lang}/Admin/Experience/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string lang, ExperienceViewModel model, string? contentLang = null, string? type = null)
        {
            // contentLang önce route parametresinden, sonra form'dan, sonra query string'den al
            var currentLang = contentLang ?? Request.Form["contentLang"].ToString() ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            // type önce route parametresinden, sonra form'dan al
            var experienceType = type ?? Request.Form["type"].ToString() ?? Request.Form["Type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            // Validation: kategoriye göre zorunlu alanları ayarla
            // Diller: Company, DateRange, Description zorunlu olmamalı
            // Eğitim: Company zorunlu olmamalı
            if (experienceType == "Diller")
            {
                ModelState.Remove(nameof(ExperienceViewModel.Company));
                ModelState.Remove(nameof(ExperienceViewModel.DateRange));
                ModelState.Remove(nameof(ExperienceViewModel.Description));
                ModelState.Remove(nameof(ExperienceViewModel.TagsInput));
            }
            else if (experienceType == "Eğitim")
            {
                ModelState.Remove(nameof(ExperienceViewModel.Company));
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
                // EN tarafında type değerleri İngilizce olabileceği için, bellek üzerinde TR'ye normalize et
                NormalizeSectionTypesForLanguage(sections, currentLang);
                
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

                // Section tipine göre ekle (YENİ KAYIT EN ÜSTE GELSİN)
                if (experienceType == "Eğitim" || experienceType == "Diller")
                {
                    section.Items ??= new List<ExperienceViewModel>();
                    section.Items.Insert(0, model); // en üste ekle
                }
                else
                {
                    section.Experience ??= new List<ExperienceViewModel>();
                    section.Experience.Insert(0, model); // en üste ekle
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
            // contentLang önce route parametresinden, sonra query string'den al
            var currentLang = contentLang ?? Request.Query["contentLang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = lang ?? "tr";
            }

            var experienceType = type ?? Request.Query["type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
            // EN JSON içindeki type değerlerini TR karşılıklarına normalize et
            NormalizeSectionTypesForLanguage(sections, currentLang);
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
            // contentLang önce route parametresinden, sonra form'dan, sonra query string'den al
            var currentLang = contentLang ?? Request.Form["contentLang"].ToString() ?? Request.Query["lang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = "tr";
            }

            // type önce route parametresinden, sonra form'dan al
            var experienceType = type ?? Request.Form["type"].ToString() ?? Request.Form["Type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            // Validation: kategoriye göre zorunlu alanları ayarla
            if (experienceType == "Diller")
            {
                ModelState.Remove(nameof(ExperienceViewModel.Company));
                ModelState.Remove(nameof(ExperienceViewModel.DateRange));
                ModelState.Remove(nameof(ExperienceViewModel.Description));
                ModelState.Remove(nameof(ExperienceViewModel.TagsInput));
            }
            else if (experienceType == "Eğitim")
            {
                ModelState.Remove(nameof(ExperienceViewModel.Company));
            }

            try
            {
                var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
                // EN JSON içindeki type değerlerini TR karşılıklarına normalize et
                NormalizeSectionTypesForLanguage(sections, currentLang);
                var section = sections.FirstOrDefault(s => s.Type == experienceType);
                
                if (section == null)
                {
                    return NotFound();
                }

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

                // Section tipine göre güncelle + ID swap mantığı
                if (experienceType == "Eğitim" || experienceType == "Diller")
                {
                    if (section.Items == null) section.Items = new List<ExperienceViewModel>();

                    var items = section.Items;
                    var oldId = id;
                    var newId = model.Id;

                    var itemIndex = items.FindIndex(e => e.Id == oldId);
                    if (itemIndex == -1) return NotFound();

                    // Eğer yeni ID, aynı listede başka bir kayıt tarafından kullanılıyorsa, ID'leri takas et
                    if (newId != oldId)
                    {
                        var other = items.FirstOrDefault(e => e.Id == newId);
                        if (other != null)
                        {
                            other.Id = oldId;
                        }
                    }

                    items[itemIndex] = model; // model.Id zaten formdan gelen yeni değeri taşıyor
                }
                else
                {
                    if (section.Experience == null) section.Experience = new List<ExperienceViewModel>();

                    var exps = section.Experience;
                    var oldId = id;
                    var newId = model.Id;

                    var expIndex = exps.FindIndex(e => e.Id == oldId);
                    if (expIndex == -1) return NotFound();

                    // Eğer yeni ID, aynı listede başka bir kayıt tarafından kullanılıyorsa, ID'leri takas et
                    if (newId != oldId)
                    {
                        var other = exps.FirstOrDefault(e => e.Id == newId);
                        if (other != null)
                        {
                            other.Id = oldId;
                        }
                    }

                    exps[expIndex] = model;
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
            // contentLang önce route parametresinden, sonra form'dan, sonra query string'den al
            var currentLang = contentLang ?? Request.Form["contentLang"].ToString() ?? Request.Query["contentLang"].ToString().ToLower();
            if (string.IsNullOrEmpty(currentLang) || (currentLang != "tr" && currentLang != "en"))
            {
                currentLang = lang ?? "tr";
            }

            var experienceType = type ?? Request.Form["type"].ToString() ?? Request.Query["type"].ToString();
            if (string.IsNullOrEmpty(experienceType))
            {
                experienceType = "Eğitim";
            }

            try
            {
                var sections = await _jsonService.ReadJsonArrayAsync<ExperienceSectionViewModel>("experience.json", currentLang);
                // EN JSON içindeki type değerlerini TR karşılıklarına normalize et
                NormalizeSectionTypesForLanguage(sections, currentLang);
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
