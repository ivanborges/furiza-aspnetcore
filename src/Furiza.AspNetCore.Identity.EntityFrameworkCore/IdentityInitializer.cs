using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    internal class IdentityInitializer
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IdentityConfiguration identityConfiguration;

        public IdentityInitializer(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IdentityConfiguration identityConfiguration)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.identityConfiguration = identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration));
        }
        
        public void Initialize()
        {
            foreach (FieldInfo fieldInfo in typeof(FurizaMasterRoles).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                if (!roleManager.RoleExistsAsync(fieldInfo.Name).Result)
                {
                    var resultado = roleManager.CreateAsync(new ApplicationRole()
                    {
                        Name = fieldInfo.Name,
                        CreationDate = DateTime.UtcNow,
                        CreationUser = "superuser"
                    }).Result;
                    if (!resultado.Succeeded)
                        throw new InvalidOperationException($"An error occurred while creating the role {fieldInfo.Name}.");
                }

            CreateUser(new ApplicationUser()
            {
                UserName = "superuser",
                FullName = "Superuser",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                HiringType = FurizaHiringTypes.InHouse,
                Company = "furiza",
                Department = "prez",
                CreationDate = DateTime.UtcNow,
                CreationUser = "superuser"
            }, new string[] { FurizaMasterRoles.Superuser });
            CreateUser(new ApplicationUser()
            {
                UserName = "admin",
                FullName = "Administrator",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                HiringType = FurizaHiringTypes.InHouse,
                Company = "furiza",
                Department = "prez",
                CreationDate = DateTime.UtcNow,
                CreationUser = "superuser"
            }, new string[] { FurizaMasterRoles.Administrator });
            CreateUser(new ApplicationUser()
            {
                UserName = "editor",
                FullName = "Editor",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                HiringType = FurizaHiringTypes.InHouse,
                Company = "furiza",
                Department = "prez",
                CreationDate = DateTime.UtcNow,
                CreationUser = "superuser"
            }, new string[] { FurizaMasterRoles.Editor });
            CreateUser(new ApplicationUser()
            {
                UserName = "approver",
                FullName = "Approver",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                HiringType = FurizaHiringTypes.InHouse,
                Company = "furiza",
                Department = "prez",
                CreationDate = DateTime.UtcNow,
                CreationUser = "superuser"
            }, new string[] { FurizaMasterRoles.Approver });
            CreateUser(new ApplicationUser()
            {
                UserName = "user",
                FullName = "Basic User",
                Email = identityConfiguration.DefaultEmailAddress,
                EmailConfirmed = true,
                HiringType = FurizaHiringTypes.InHouse,
                Company = "furiza",
                Department = "prez",
                CreationDate = DateTime.UtcNow,
                CreationUser = "superuser"
            });
        }

        private void CreateUser(ApplicationUser user, string[] additionalRoles = null)
        {
            if (userManager.FindByNameAsync(user.UserName).Result == null)
            {
                var creationResult = userManager.CreateAsync(user, user.UserName.ToLower()).Result;
                if (creationResult.Succeeded)
                {
                    userManager.AddClaimAsync(user, new Claim(FurizaClaimNames.SystemUser, "")).Wait();
                    userManager.AddToRoleAsync(user, FurizaMasterRoles.Viewer).Wait();

                    if (additionalRoles != null)
                        foreach (var role in additionalRoles)
                            userManager.AddToRoleAsync(user, role).Wait();
                }
            }
        }
    }
}