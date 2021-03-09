using Microsoft.AspNetCore.Mvc;

namespace CheaperResearch.Controllers
{
    public class ModulesController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}