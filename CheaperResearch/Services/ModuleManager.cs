using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheaperResearch.Data;
using CheaperResearch.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CheaperResearch.Services
{
    public class ModuleManager
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ModuleManager(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Module>> GetModules(ModuleQuery query)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            return await GetModules(query, dbContext);
        }

        public async Task<List<Module>> GetModules(ModuleQuery query, ApplicationDbContext dbContext)
        {
            var queryable = dbContext.Modules
                .Include(module => module.ParentModule)
                .Include(module => module.ModuleUsers)
                .ThenInclude(user => user.ApplicationUser)
                .Include(module => module.ModuleResults)
                .Include(module => module.Modules)
                .AsQueryable();

            if (query.Name is not null)
            {
                queryable = queryable.Where(module => module.Name.Contains(query.Name));
            }

            if (query.Id is not null)
            {
                queryable = queryable.Where(module => query.Id.Contains(module.Id));
            }

            if (query.ParentModule is not null)
            {
                queryable = queryable.Where(module => query.ParentModule.Contains(module.ParentModuleId));
            }

            if (query.ModuleUserRoles is not null)
            {
                return queryable.AsEnumerable().Where(module => module.ModuleUsers.Any(user =>
                        query.ModuleUserRoles.Keys.Contains(user.ApplicationUserId) &&
                        query.ModuleUserRoles[user.ApplicationUserId].Contains(user.Role)))
                    .ToList();
            }

            return await queryable.ToListAsync();
        }

        public async Task<Module> CreateModule(CreateModule module)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();


            try
            {
                var result = new Module()
                {
                    Id = module.Id,
                    Name = module.Name,
                    ParentModuleId = module.ParentId,
                    ModuleUsers = new List<ModuleUser>()
                };
                var moduleUsers = await dbContext.ModuleUsers.Where(user => user.ModuleId == module.ParentId).ToListAsync();
                foreach (var moduleUser in moduleUsers)
                {
                    result.ModuleUsers.Add(new ModuleUser()
                    {
                        Locked = moduleUser.Locked,
                        Role = moduleUser.Role,
                        Module = result,
                        ApplicationUserId = moduleUser.ApplicationUserId
                    });
                    
                }
                
                await dbContext.AddAsync(result);
                await dbContext.SaveChangesAsync();
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task RemoveModule(string moduleId)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            var module = (await GetModules(new ModuleQuery()
            {
                Id = new[] {moduleId}
            }, dbContext)).SingleOrDefault();
            if (module is not null)
            {
                dbContext.Remove(module);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateModule(Module updateModule)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            dbContext.Update(updateModule);


            await dbContext.SaveChangesAsync();
        }

        public async Task RecordResult(ModuleResult previousResult)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            if (string.IsNullOrEmpty(previousResult.Id))
            {
                await
                    dbContext.AddAsync(previousResult);
            }
            else
            {
                dbContext.Update(previousResult);
            }

            await dbContext.SaveChangesAsync();
        }
    }

    public class ModuleQuery
    {
        public string Name { get; set; }
        public string[] Id { get; set; }
        public string[] ParentModule { get; set; }

        public Dictionary<string, ModuleUserRole[]> ModuleUserRoles { get; set; }
    }
}