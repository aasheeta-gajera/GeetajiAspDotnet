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
        List<Language> languages = LoadJsonData();
        Language currentShloka = null;
        int currentIndex = -1;
        int totalShlokas = 0;

        // Get liked shlokas from session
        var likedShlokas = HttpContext.Session.GetString("LikedShlokas") ?? "[]";
        var likedIds = JsonConvert.DeserializeObject<List<int>>(likedShlokas);

        foreach (var lang in languages)
        {
            if (lang.Data != null)
            {
                currentIndex = lang.Data.FindIndex(item => item.Id == id);
                if (currentIndex != -1)
                {
                    currentShloka = lang.Data[currentIndex];
                    currentShloka.IsLiked = likedIds.Contains(id);
                    totalShlokas = lang.Data.Count;
                    break;
                }
            }
        }

        if (currentShloka != null)
        {
            ViewBag.CurrentIndex = currentIndex;
            ViewBag.TotalShlokas = totalShlokas;
            ViewBag.PreviousId = currentIndex > 0 ? languages[0].Data[currentIndex - 1].Id : -1;
            ViewBag.NextId = currentIndex < totalShlokas - 1 ? languages[0].Data[currentIndex + 1].Id : -1;
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

    [HttpPost]
    public IActionResult ToggleLike([FromBody] LikeRequest request)
    {
        var likedShlokas = HttpContext.Session.GetString("LikedShlokas");
        List<int> likedIds;
        
        if (string.IsNullOrEmpty(likedShlokas))
        {
            likedIds = new List<int>();
        }
        else
        {
            likedIds = JsonConvert.DeserializeObject<List<int>>(likedShlokas);
        }

        // Toggle like status
        if (likedIds.Contains(request.ShlokaId))
        {
            likedIds.Remove(request.ShlokaId);
        }
        else
        {
            likedIds.Add(request.ShlokaId);
        }

        // Save back to session
        HttpContext.Session.SetString("LikedShlokas", JsonConvert.SerializeObject(likedIds));
        return Json(new { success = true, isLiked = likedIds.Contains(request.ShlokaId) });
    }

    public IActionResult Favorites()
    {
        var likedShlokas = HttpContext.Session.GetString("LikedShlokas");
        List<int> likedIds;
        
        if (string.IsNullOrEmpty(likedShlokas))
        {
            likedIds = new List<int>();
        }
        else
        {
            likedIds = JsonConvert.DeserializeObject<List<int>>(likedShlokas);
        }

        var languages = LoadJsonData();
        var favoriteShlokas = new List<Language>();

        // Use a HashSet to track unique shloka IDs
        var uniqueIds = new HashSet<int>(likedIds);

        foreach (var lang in languages)
        {
            if (lang.Data != null)
            {
                // Only add shlokas that are in our unique liked IDs
                favoriteShlokas.AddRange(lang.Data.Where(s => s.Id.HasValue && uniqueIds.Contains(s.Id.Value)));
            }
        }

        // Clear any existing liked shlokas and set only the unique ones
        HttpContext.Session.SetString("LikedShlokas", JsonConvert.SerializeObject(uniqueIds.ToList()));

        return View(favoriteShlokas);
    }
}

public class LikeRequest
{
    public int ShlokaId { get; set; }
}

