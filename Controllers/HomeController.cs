using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class HomeController : Controller
{
    private List<Language> LoadJsonData()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "languages.json");
        string jsonData = System.IO.File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<Language>>(jsonData);
    }

    public IActionResult Index()
    {
        List<Language> languages = LoadJsonData();
        return View(languages);
    }

    public IActionResult ShowNames(int index)
    {
        List<Language> languages = LoadJsonData();
        if (index < languages.Count)
        {
            ViewBag.ChapterName = languages[index].Name1;
            ViewBag.TotalShlokas = languages[index].Data?.Count ?? 0;
            return View(languages[index].Data);
        }
        return RedirectToAction("Index");
    }

    public IActionResult ShowShloka(int id)
    {
        var languages = LoadJsonData();
        Language? currentShloka = null;
        var currentIndex = -1;
        var totalShlokas = 0;
        var currentLanguage = languages.FirstOrDefault();

        // Find the current shloka and its index
        foreach (var lang in languages)
        {
            if (lang?.Data != null)
            {
                var shloka = lang.Data.FirstOrDefault(s => s?.Id == id);
                if (shloka != null)
                {
                    currentShloka = shloka;
                    currentIndex = lang.Data.FindIndex(s => s?.Id == id);
                    totalShlokas = lang.Data.Count;
                    ViewBag.ChapterName = lang.Name1 ?? "Unknown Chapter";
                    currentLanguage = lang;
                    break;
                }
            }
        }

        if (currentShloka != null && currentLanguage?.Data != null)
        {
            ViewBag.CurrentIndex = currentIndex;
            ViewBag.TotalShlokas = totalShlokas;
            
            // Handle previous ID
            int previousId = -1;
            if (currentIndex > 0 && currentLanguage.Data.Count > currentIndex - 1)
            {
                var prevShloka = currentLanguage.Data[currentIndex - 1];
                if (prevShloka?.Id.HasValue == true)
                {
                    previousId = prevShloka.Id.Value;
                }
            }
            ViewBag.PreviousId = previousId;

            // Handle next ID
            int nextId = -1;
            if (currentIndex < totalShlokas - 1 && currentLanguage.Data.Count > currentIndex + 1)
            {
                var nextShloka = currentLanguage.Data[currentIndex + 1];
                if (nextShloka?.Id.HasValue == true)
                {
                    nextId = nextShloka.Id.Value;
                }
            }
            ViewBag.NextId = nextId;

            return View(currentShloka);
        }

        return RedirectToAction("Index");
    }

    public IActionResult ShlokaDetails(int id)
    {
        var languages = LoadJsonData();
        var selectedLanguage = languages.FirstOrDefault(lang => lang.Id == id);

        if (selectedLanguage != null && selectedLanguage.Data != null)
        {
            return View(selectedLanguage.Data);
        }

        return NotFound();
    }
}

public class LikeRequest
{
    public int ShlokaId { get; set; }
}

