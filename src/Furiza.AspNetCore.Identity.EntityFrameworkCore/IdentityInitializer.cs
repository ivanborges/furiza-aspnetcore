using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    internal class IdentityInitializer
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
                Company = "furiza",
                Department = "prez"
            }, "superuser", ApplicationUserType.System, Role.Superuser);
            CreateUser(new ApplicationUser()
            {
                UserName = "admin",
                FullName = "Administrator",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                Company = "furiza",
                Department = "prez"
            }, "admin", ApplicationUserType.System, Role.Administrator);
            CreateUser(new ApplicationUser()
            {
                UserName = "user",
                FullName = "Common User",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                Company = "furiza",
                Department = "prez"
            }, "user", ApplicationUserType.System, Role.User);
        }

        private void CreateUser(ApplicationUser user, string password, ApplicationUserType applicationUserType, Role? initialRole = null)
        {
            if (userManager.FindByNameAsync(user.UserName).Result == null)
            {
                var creationResult = userManager.CreateAsync(user, password).Result;
                if (creationResult.Succeeded)
                {
                    userManager.AddClaimAsync(user, new Claim(ClaimTypesCustom.UserType, applicationUserType.ToString())).Wait();

                    if (initialRole != null)
                        userManager.AddToRoleAsync(user, initialRole.Value.ToString()).Wait();
                }
            }
        }
    }
}