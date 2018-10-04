using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityInitializer
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IdentityConfiguration identityConfiguration;

        public IdentityInitializer(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IdentityConfiguration identityConfiguration)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.identityConfiguration = identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration));
        }

        public void Initialize()
        {
            if (!dbContext.Database.EnsureCreated())
                return;
            
            foreach (var entry in Enum.GetValues(typeof(Role)))
                if (!roleManager.RoleExistsAsync(entry.ToString()).Result)
                {
                    var resultado = roleManager.CreateAsync(new ApplicationRole() { Name = entry.ToString() }).Result;
                    if (!resultado.Succeeded)
                        throw new Exception($"An error occurred while creating the role {entry.ToString()}.");
                }

            CreateUser(new ApplicationUser()
            {
                UserName = "superuser",
                FullName = "Superuser",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                Company = "Furiza",
                Department = "PREZ"
            }, "superuser", Role.Superuser);
            CreateUser(new ApplicationUser()
            {
                UserName = "admin",
                FullName = "Administrator",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                Company = "Furiza",
                Department = "PREZ"
            }, "admin", Role.Administrator);
            CreateUser(new ApplicationUser()
            {
                UserName = "user",
                FullName = "Common User",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                Company = "Furiza",
                Department = "PREZ"
            }, "user", Role.User);
        }

        private void CreateUser(ApplicationUser user, string password, Role? initialRole = null)
        {
            if (userManager.FindByNameAsync(user.UserName).Result == null)
            {
                var resultado = userManager.CreateAsync(user, password).Result;
                if (resultado.Succeeded && initialRole != null)
                    userManager.AddToRoleAsync(user, initialRole.Value.ToString()).Wait();
            }
        }
    }
}