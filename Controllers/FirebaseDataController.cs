using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class FirebaseDataController : Controller
{
    private readonly FirebaseService _firebaseService;

    public FirebaseDataController(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<IActionResult> Index()
    {
        var languages = await _firebaseService.GetAllLanguages();
        return View(languages);
    }
} 