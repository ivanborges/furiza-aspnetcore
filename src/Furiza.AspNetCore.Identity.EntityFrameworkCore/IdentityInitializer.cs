using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;

        public IdentityInitializer(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IdentityConfiguration identityConfiguration,
            ILoggerFactory loggerFactory)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.identityConfiguration = identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration));
            logger = loggerFactory?.CreateLogger<IdentityInitializer>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }
        
        public void Initialize()
        {
            if (!identityConfiguration.EnableInitializer)
            {
                logger.LogInformation("Identity initializer disabled.");
                return;
            }

            try
            {
                logger.LogInformation("Filling database with system users and roles...");

                foreach (FieldInfo fieldInfo in typeof(FurizaMasterRoles).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                    if (!roleManager.RoleExistsAsync(fieldInfo.Name).Result)
                    {
                        var resultado = roleManager.CreateAsync(new ApplicationRole()
                        {
                            Name = fieldInfo.Name
                        }).Result;

                        if (resultado.Succeeded)
                            logger.LogInformation($"Role '{fieldInfo.Name}' created.");
                        else
                            logger.LogWarning($"An error occurred while creating the role '{fieldInfo.Name}'.");
                    }

                CreateUser(new ApplicationUser()
                {
                    UserName = "superuser",
                    FullName = "Superuser",
                    EmailConfirmed = true
                }, new string[] { FurizaMasterRoles.Superuser });
                CreateUser(new ApplicationUser()
                {
                    UserName = "admin",
                    FullName = "Administrator",
                    EmailConfirmed = true
                }, new string[] { FurizaMasterRoles.Administrator });
                CreateUser(new ApplicationUser()
                {
                    UserName = "editor",
                    FullName = "Editor",
                    EmailConfirmed = true
                }, new string[] { FurizaMasterRoles.Editor });
                CreateUser(new ApplicationUser()
                {
                    UserName = "approver",
                    FullName = "Approver",
                    EmailConfirmed = true
                }, new string[] { FurizaMasterRoles.Approver });
                CreateUser(new ApplicationUser()
                {
                    UserName = "user",
                    FullName = "Basic User",
                    EmailConfirmed = true
                });

                logger.LogInformation("Database filled.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "An internal error occurred while trying to load system users and roles.");
            }
        }

        private void CreateUser(ApplicationUser user, string[] additionalRoles = null)
        {
            if (userManager.FindByNameAsync(user.UserName).Result == null)
            {
                var creationResult = userManager.CreateAsync(user, user.UserName.ToLower()).Result;
                if (creationResult.Succeeded)
                {
                    logger.LogInformation($"User '{user}' created.");

                    userManager.AddClaimAsync(user, new Claim(FurizaClaimNames.SystemUser, "")).Wait();
                    userManager.AddToRoleAsync(user, FurizaMasterRoles.Viewer).Wait();

                    if (additionalRoles != null)
                        foreach (var role in additionalRoles)
                            userManager.AddToRoleAsync(user, role).Wait();

                    logger.LogInformation("Default roles and claims set to user.");
                }
                else
                    logger.LogWarning($"An error occurred while creating the user '{user}'.");
            }
        }
    }
}