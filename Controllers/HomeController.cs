using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class HomeController : Controller
{
    private readonly FirebaseService _firebaseService;

    public HomeController(IConfiguration configuration)
    {
        _firebaseService = new FirebaseService(configuration);
    }

    public async Task<IActionResult> Index()
    {
        var languages = await _firebaseService.GetAllLanguages();
        return View(languages);
    }

    public async Task<IActionResult> ShowNames(int chapterId)
    {
        var languages = await _firebaseService.GetAllLanguages();
        var chapter = languages.FirstOrDefault(c => c.Id == chapterId);
        
        if (chapter != null)
        {
            ViewBag.ChapterName = chapter.Name1;
            ViewBag.TotalShlokas = chapter.Data?.Count ?? 0;
            ViewBag.ChapterId = chapter.Id;
            return View(chapter.Data);
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ShowShloka(int id)
    {
        var languages = await _firebaseService.GetAllLanguages();
        Shloka? currentShloka = null;
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
                    ViewBag.ChapterId = lang.Id;
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
                if (prevShloka != null)
                {
                    previousId = prevShloka.Id;
                }
            }
            ViewBag.PreviousId = previousId;

            // Handle next ID
            int nextId = -1;
            if (currentIndex < totalShlokas - 1 && currentLanguage.Data.Count > currentIndex + 1)
            {
                var nextShloka = currentLanguage.Data[currentIndex + 1];
                if (nextShloka != null)
                {
                    nextId = nextShloka.Id;
                }
            }
            ViewBag.NextId = nextId;
            
            // Handle previous and next chapters
            int currentChapterIndex = languages.FindIndex(l => l.Id == currentLanguage.Id);
            
            // Previous chapter
            int prevChapterId = -1;
            if (currentChapterIndex > 0)
            {
                prevChapterId = languages[currentChapterIndex - 1].Id;
            }
            ViewBag.PreviousChapterId = prevChapterId;
            
            // Next chapter
            int nextChapterId = -1;
            if (currentChapterIndex < languages.Count - 1)
            {
                nextChapterId = languages[currentChapterIndex + 1].Id;
            }
            ViewBag.NextChapterId = nextChapterId;

            return View(currentShloka);
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ShlokaDetails(int id)
    {
        var shloka = await _firebaseService.GetShlokaById(id);
        if (shloka != null)
        {
            return View(shloka);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int shlokaId)
    {
        var shloka = await _firebaseService.GetShlokaById(shlokaId);
        if (shloka != null)
        {
            shloka.IsLiked = !shloka.IsLiked;
            await _firebaseService.UpdateShloka(shloka);
        }
        return RedirectToAction("ShlokaDetails", new { id = shlokaId });
    }

    public async Task<IActionResult> Favorites()
    {
        var languages = await _firebaseService.GetAllLanguages();
        var favoriteShlokas = languages.SelectMany(l => l.Data)
                                     .Where(s => s.IsLiked)
                                     .ToList();
        return View(favoriteShlokas);
    }
}
