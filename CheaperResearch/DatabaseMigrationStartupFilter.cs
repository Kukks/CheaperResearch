using System;
using CheaperResearch.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CheaperResearch
{
    public class DatabaseMigrationStartupFilter:IStartupFilter
    {


        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                using var context = builder.ApplicationServices
                    .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                    .CreateDbContext();
                context.Database.EnsureCreated();
            };
        }
    }
}