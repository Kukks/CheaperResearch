using System.Linq;
using System.Threading.Tasks;
using CheaperResearch.Data;
using CheaperResearch.Models;
using CheaperResearch.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheaperResearch
{
    [Route("management/modules")]
    public class ModulesController : Controller
    {
        private readonly ModuleManager _moduleManager;

        public ModulesController(ModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetModules(ModuleQuery query)
        {
            if (query == default)
            {
                query.ParentModule = new string[] {null};
            }

            return View(await _moduleManager.GetModules(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetModule(string id)
        {
            var result = (await _moduleManager.GetModules(new ModuleQuery()
            {
                Id = new[] {id}
            })).SingleOrDefault();

            if (result is null)
            {
                return NotFound();
            }

            return View(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateModule(Module module)
        {
            await _moduleManager.UpdateModule(module);
            return RedirectToAction(nameof(GetModule), new {module.Id});
        }


        [HttpGet("create")]
        public async Task<IActionResult> CreateModule()
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateModule(CreateModule request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = (await _moduleManager.CreateModule(request));
            if (result is null)
            {
                ModelState.AddModelError("", "Invalid id");

                return View(request);
            }

            return RedirectToAction(nameof(GetModule), new {result.Id});
        }
    }
}