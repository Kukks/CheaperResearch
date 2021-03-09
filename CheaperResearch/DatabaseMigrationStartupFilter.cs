using System;
using CheaperResearch.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CheaperResearch
{
    public class DatabaseMigrationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                using var context = builder.ApplicationServices
                    .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                    .CreateDbContext();
                context.Database.Migrate();
                next(builder);
            };
        }
    }
    
    public class RolesCreationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                using (var scope = builder.ApplicationServices.CreateScope())
                {
                    var roleManager = scope.ServiceProvider
                        .GetRequiredService<RoleManager<IdentityRole>>();
                    roleManager.CreateAsync(new IdentityRole(Constants.ServerAdminRole)).ConfigureAwait(true).GetAwaiter()
                        .GetResult();
                    roleManager.CreateAsync(new IdentityRole(Constants.ModuleAdminRole)).ConfigureAwait(true).GetAwaiter()
                        .GetResult();
                    next(builder);
                }
               
            };
        }
    }
}