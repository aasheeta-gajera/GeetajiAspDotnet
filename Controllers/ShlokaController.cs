using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class ShlokaController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Shloka>> GetShlokas()
    {
        var shlokas = new List<Shloka>
        {
            new Shloka { Id = 1, ChapterName = "अर्जुनविषादयोग", TotalShlokas = 47, ShlokaNumber = 1, ShlokaText = "श्लोक: 1" },
new Shloka { Id = 2, ChapterName = "सांख्ययोग", TotalShlokas = 72, ShlokaNumber = 2, ShlokaText = "श्लोक: 2" },
new Shloka { Id = 3, ChapterName = "कर्मयोग", TotalShlokas = 43, ShlokaNumber = 3, ShlokaText = "श्लोक: 3" },
new Shloka { Id = 4, ChapterName = "ज्ञानकर्मसन्यासयोग", TotalShlokas = 42, ShlokaNumber = 4, ShlokaText = "श्लोक: 4" },
new Shloka { Id = 5, ChapterName = "कर्मसंन्यासयोग", TotalShlokas = 29, ShlokaNumber = 5, ShlokaText = "श्लोक: 5" },
new Shloka { Id = 6, ChapterName = "आत्मसंयमयोग", TotalShlokas = 47, ShlokaNumber = 6, ShlokaText = "श्लोक: 6" },
new Shloka { Id = 7, ChapterName = "ज्ञानविज्ञानयोग", TotalShlokas = 30, ShlokaNumber = 7, ShlokaText = "श्लोक: 7" },
new Shloka { Id = 8, ChapterName = "अक्षरब्रह्मयोग", TotalShlokas = 28, ShlokaNumber = 8, ShlokaText = "श्लोक: 8" },
new Shloka { Id = 9, ChapterName = "राजविद्याराजगुह्ययोग", TotalShlokas = 34, ShlokaNumber = 9, ShlokaText = "श्लोक: 9" },
new Shloka { Id = 10, ChapterName = "विभूतियोग", TotalShlokas = 42, ShlokaNumber = 10, ShlokaText = "श्लोक: 10" },
new Shloka { Id = 11, ChapterName = "विश्वरूपदर्शनयोग", TotalShlokas = 55, ShlokaNumber = 11, ShlokaText = "श्लोक: 11" },
new Shloka { Id = 12, ChapterName = "भक्तियोग", TotalShlokas = 20, ShlokaNumber = 12, ShlokaText = "श्लोक: 12" },
new Shloka { Id = 13, ChapterName = "क्षेत्रक्षत्रज्ञविभागयोग", TotalShlokas = 35, ShlokaNumber = 13, ShlokaText = "श्लोक: 13" },
new Shloka { Id = 14, ChapterName = "गुणत्रयविभागयोग", TotalShlokas = 27, ShlokaNumber = 14, ShlokaText = "श्लोक: 14" },
new Shloka { Id = 15, ChapterName = "पुरुषोत्तमयोग", TotalShlokas = 20, ShlokaNumber = 15, ShlokaText = "श्लोक: 15" },
new Shloka { Id = 16, ChapterName = "दैवासुरसंपद्विभागयोग", TotalShlokas = 24, ShlokaNumber = 16, ShlokaText = "श्लोक: 16" },
new Shloka { Id = 17, ChapterName = "श्रद्धात्रयविभागयोग", TotalShlokas = 28, ShlokaNumber = 17, ShlokaText = "श्लोक: 17" },
new Shloka { Id = 18, ChapterName = "मोक्षसंन्यासयोग", TotalShlokas = 78, ShlokaNumber = 18, ShlokaText = "श्लोक: 18" }

        };

        return Ok(shlokas);
    }
}
