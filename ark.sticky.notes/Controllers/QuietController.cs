using Microsoft.AspNetCore.Mvc;

namespace ark.bible.analysis.Controllers
{
    public class QuietController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
