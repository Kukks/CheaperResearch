using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CheaperResearch.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleUser> ModuleUsers { get; set; }
        public DbSet<ModuleResult> ModuleResults { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ModuleUser>()
                .HasIndex(nameof(ModuleUser.ApplicationUserId), nameof(ModuleUser.ModuleId));
            builder.Entity<Module>().HasMany<ModuleUser>(module => module.ModuleUsers).WithOne(user => user.Module);
            builder.Entity<Module>().HasMany<Module>(module => module.Modules).WithOne(user => user.ParentModule);
            builder.Entity<ApplicationUser>().HasMany<ModuleUser>(module => module.ModuleUsers)
                .WithOne(user => user.ApplicationUser);
            base.OnModelCreating(builder);
        }
    }

    public class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=blog.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }


    public class Module
    {
        [Key] public string Id { get; set; }
        public string ParentModuleId { get; set; }
        public string Name { get; set; }
        public Module? ParentModule { get; set; }
        public List<ModuleResult> ModuleResults { get; set; }
        public List<ModuleUser> ModuleUsers { get; set; }
        
        public List<Module> Modules { get; set; }


        public string Blob { get; set; }
    }

    public class ModuleUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public string Id { get; set; }

        public string ModuleId { get; set; }
        public ModuleUserRole Role { get; set; }
        public bool Locked { get; set; }
        public Module Module { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum ModuleUserRole
    {
        User,
        Admin
    }

    public class ModuleResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public string Id { get; set; }

        public string ModuleId { get; set; }
        public string ApplicationUserId { get; set; }
        public string Blob { get; set; }
        public bool Finished { get; set; }
        public Module Module { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        public List<ModuleResult> ModuleResults { get; set; }
        public List<ModuleUser> ModuleUsers { get; set; }
    }
}