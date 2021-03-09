using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CheaperResearch.Data;
using CheaperResearch.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CheaperResearch.Controllers
{
    [Route("modules")]
    [Authorize]
    public class ModulesController : Controller
    {
        private readonly ModuleManager _moduleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModulesController(ModuleManager moduleManager, UserManager<ApplicationUser> userManager)
        {
            _moduleManager = moduleManager;
            _userManager = userManager;
        }

        [HttpGet("{parentId}")]
        [HttpGet("")]
        public async Task<IActionResult> GetModules(string parentId = null)
        {
            var modules = await _moduleManager.GetModules(new ModuleQuery()
            {
                ParentModule = new string[] {parentId},
                ModuleUserRoles = new Dictionary<string, ModuleUserRole[]>()
                {
                    {
                        GetUserId(), new ModuleUserRole[] {ModuleUserRole.User}
                    }
                }
            });
            foreach (var module in modules)
            {
                module.ModuleUsers = module.ModuleUsers
                    .Where(user => user.Role == ModuleUserRole.User && user.ApplicationUserId == GetUserId()).ToList();
                module.ModuleResults =
                    module.ModuleResults.Where(result => result.ApplicationUserId == GetUserId()).ToList();
            }
            return View(modules);
        }

        [HttpGet("{id?}/view")]
        public async Task<IActionResult> ViewModule(string id = null)
        {
            var module = (await _moduleManager.GetModules(new ModuleQuery()
            {
                Id = new[] {id},
                ModuleUserRoles = new Dictionary<string, ModuleUserRole[]>()
                {
                    {
                        GetUserId(), new ModuleUserRole[] {ModuleUserRole.User}
                    }
                }
            })).SingleOrDefault();
            if (module?.ModuleUsers?.Count(user => user.ApplicationUserId == GetUserId() && !user.Locked) == 0)
            {
                return NotFound();
            }

            module.ModuleUsers = module.ModuleUsers
                .Where(user => user.Role == ModuleUserRole.User && user.ApplicationUserId == GetUserId()).ToList();
            module.ModuleResults =
                module.ModuleResults.Where(result => result.ApplicationUserId == GetUserId()).ToList();

            return View(module);
        }

        public class RecordResult
        {
            [JsonPropertyName("blob")]
            public string Blob { get; set; }
            
            [JsonPropertyName("complete")]
            public bool Complete{ get; set; }
        }

        [HttpPost("{id?}/progress")]
        public async Task<IActionResult> RecordModuleResult(string id, [FromBody] RecordResult result)
        {
            var module = (await _moduleManager.GetModules(new ModuleQuery()
            {
                Id = new[] {id},
                ModuleUserRoles = new Dictionary<string, ModuleUserRole[]>()
                {
                    {
                        GetUserId(), new ModuleUserRole[] {ModuleUserRole.User}
                    }
                }
            })).SingleOrDefault();
            if (module?.ModuleUsers?.SingleOrDefault(user => user.ApplicationUserId == GetUserId() && !user.Locked) is null)
            {
                return NotFound();
            }

            var previousResult =
                module.ModuleResults.SingleOrDefault(result => result.ApplicationUserId == GetUserId());

            if (previousResult?.Finished is true)
            {
                return BadRequest();
            }

            previousResult ??= new ModuleResult()
            {
                Finished = result.Complete,
                ApplicationUserId = GetUserId(),
                ModuleId = id,
                Blob =  result.Blob,
            };
            previousResult.Blob = result.Blob;
            previousResult.Finished = result.Complete;
            await _moduleManager.RecordResult(previousResult);
            return RedirectToAction("GetModules", new {parentId = module.ParentModuleId});
        }

        private string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}