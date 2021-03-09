using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheaperResearch.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheaperResearch.Controllers
{
    [Authorize(Roles = Constants.ServerAdminRole)]
    
    [Route("management/users")]
    public class UsersController : Controller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IDbContextFactory<ApplicationDbContext> dbContextFactory, RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser > userManager)
        {
            _dbContextFactory = dbContextFactory;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> ListUsers()
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var roles = await context.Roles.ToListAsync();
            var serverAdminRoleId = roles.Single(role => role.Name == Constants.ServerAdminRole).Id;
            var moduleAdminRoleId = roles.Single(role => role.Name == Constants.ModuleAdminRole).Id;
            var userRoles = await context.UserRoles.ToListAsync();
            var users = await context.Users.ToListAsync();
            return View(new ListUsersVM()
            {
                Users = users,
                ModuleAdmins = userRoles.Where(role => role.RoleId == moduleAdminRoleId).Select(role => role.UserId)
                    .ToList(),
                ServerAdmins = userRoles.Where(role => role.RoleId == serverAdminRoleId).Select(role => role.UserId)
                    .ToList(),
            });
        }

        [HttpGet("{userId}/{action}/{role}")]
        public async Task<IActionResult> ChangeRole(string userId, string action, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (action == "remove")
            {
               await  _userManager.RemoveFromRoleAsync(user, role);
               if (role == Constants.ModuleAdminRole)
               {
                   
                   await using var context = _dbContextFactory.CreateDbContext();
                   context.ModuleUsers.RemoveRange(await
                       context.ModuleUsers.Where(moduleUser => moduleUser.ApplicationUserId == userId && moduleUser.Role == ModuleUserRole.Admin).ToArrayAsync());
                   await context.SaveChangesAsync();
               }
            }
            else
            {                
                await  _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction("ListUsers");
        }
    }

    public class ListUsersVM
    {
        public List<ApplicationUser> Users { get; set; }
        public List<string> ModuleAdmins { get; set; }
        public List<string> ServerAdmins { get; set; }
    }
}