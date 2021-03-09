using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheaperResearch.Data;
using CheaperResearch.Models;
using CheaperResearch.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;

namespace CheaperResearch
{
    [Route("management/modules")]
    [Authorize(Roles = Constants.ServerAdminRole+","+Constants.ModuleAdminRole)]
    public class ModulesManagementController : Controller
    {
        private readonly ModuleManager _moduleManager;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ModulesManagementController(ModuleManager moduleManager, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _moduleManager = moduleManager;
            _dbContextFactory = dbContextFactory;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetModules()
        {
            var query = new ModuleQuery()
            {
                ParentModule = new string[] {null}
            };

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
        
        


        [HttpGet("{id}/delete")]
        public async Task<IActionResult> RemoveModule(string id)
        {
            await _moduleManager.RemoveModule(id);
            
            return RedirectToAction("GetModules");
        }
        

        [HttpPost("users")]
        public async Task<IActionResult> UpdateModuleUsers(ModuleUser moduleUser,string command = "")
        {
            if (moduleUser is null)
            {                
                return RedirectToAction(nameof(GetModules), new {});
            }

            await using var context = _dbContextFactory.CreateDbContext();
            if (string.IsNullOrEmpty(moduleUser.Id))
            {
                await context.ModuleUsers.AddAsync(moduleUser);
            }else if (command == "remove")
            {
                var existing = context.ModuleUsers.FindAsync(moduleUser.Id);
                context.Remove(existing);
            }
            else if(command == "add")
            {
                await context.AddAsync(moduleUser);
            }
            else
            {
                
                context.Update(moduleUser);
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(GetModule), new {id =moduleUser.ModuleId});
        }
        

        [HttpGet("create")]
        public async Task<IActionResult> CreateModule(string parentId)
        {
            return View( new CreateModule()
            {
                ParentId = parentId
            });
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
                ModelState.AddModelError(nameof(request.Id), "Id must be unique");

                return View(request);
            }

            return RedirectToAction(nameof(GetModule), new {result.Id});
        }
    }
    
    
}