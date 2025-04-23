using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

public class MigrationController : Controller
{
    private readonly FirebaseMigrationService _migrationService;

    public MigrationController(IConfiguration configuration)
    {
        _migrationService = new FirebaseMigrationService(configuration);
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Debug()
    {
        try
        {
            var firebaseService = new FirebaseService(HttpContext.RequestServices.GetRequiredService<IConfiguration>());
            var languages = await firebaseService.GetAllLanguages();
            var allShlokas = languages.SelectMany(l => l.Data).ToList();
            
            var minId = allShlokas.Any() ? allShlokas.Min(s => s.Id) : 0;
            var maxId = allShlokas.Any() ? allShlokas.Max(s => s.Id) : 0;
            
            var result = new
            {
                TotalLanguages = languages.Count,
                TotalShlokas = allShlokas.Count,
                IdRange = $"{minId} to {maxId}",
                FirstFewIds = allShlokas.OrderBy(s => s.Id).Take(10).Select(s => s.Id).ToList(),
                LastFewIds = allShlokas.OrderByDescending(s => s.Id).Take(10).Select(s => s.Id).ToList()
            };
            
            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Migrate()
    {
        try
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "languages.json");
            await _migrationService.MigrateDataFromJson(jsonFilePath);
            return Json(new { success = true, message = "Data migrated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
} 