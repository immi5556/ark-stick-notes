using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ark.bible.analysis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Editor()
        {
            return View();
        }

        public IActionResult Admin()
        {
            var master_data = FileHelper.GetMasterData();
            ViewBag.data = master_data.back_translsation.data;
            return View();
        }
    }
}