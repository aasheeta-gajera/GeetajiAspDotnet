using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class HomeController : Controller
{
    private List<Language> LoadJsonData()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "languages.json");
        string jsonData = System.IO.File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<Language>>(jsonData);
    }

    public IActionResult SplashScreen()
    {
        // Show splash screen only if session variable is not set
        if (HttpContext.Session.GetString("SplashShown") == null)
        {
            HttpContext.Session.SetString("SplashShown", "true"); // Set session flag
            return View();
        }

        // If session is already set, redirect directly to Index
        return RedirectToAction("Index");
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
            return View(languages[index].Data);
        }
        return RedirectToAction("Index");
    }

    public IActionResult ShowShloka(int id)
    {
        List<Language> languages = LoadJsonData();
        foreach (var lang in languages)
        {
            foreach (var item in lang.Data ?? new List<Language>())
            {
                if (item.Id == id)
                {
                    return View(item);
                }
            }
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
