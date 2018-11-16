using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityInitializer
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
        
        public void InitializeRoles()
        {
            if (!identityConfiguration.EnableInitializer)
            {
                logger.LogInformation("Identity initializer disabled.");
                return;
            }

            try
            {
                logger.LogInformation("Filling database with roles...");

                foreach (FieldInfo fieldInfo in typeof(FurizaMasterRoles).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                    if (!roleManager.RoleExistsAsync(fieldInfo.GetValue(typeof(FurizaMasterRoles)).ToString()).Result)
                    {
                        var resultado = roleManager.CreateAsync(new ApplicationRole()
                        {
                            Name = fieldInfo.GetValue(typeof(FurizaMasterRoles)).ToString()
                        }).Result;

                        if (resultado.Succeeded)
                            logger.LogInformation($"Role '{fieldInfo.Name}' created.");
                        else
                            logger.LogWarning($"An error occurred while creating the role '{fieldInfo.Name}'.");
                    }

                logger.LogInformation("Database filled.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "An internal error occurred while trying to load roles.");
            }
        }

        public async Task InitializeUsersAsync()
        {
            if (!identityConfiguration.EnableInitializer)
            {
                logger.LogInformation("Identity initializer disabled.");
                return;
            }

            logger.LogInformation("Filling database with system users...");

            await CreateUserAsync(new ApplicationUser()
            {
                UserName = "superuser",
                FullName = "Superuser",
                EmailConfirmed = true
            }, new string[] { FurizaMasterRoles.Superuser });
            await CreateUserAsync(new ApplicationUser()
            {
                UserName = "admin",
                FullName = "Administrator",
                EmailConfirmed = true
            }, new string[] { FurizaMasterRoles.Administrator });
            await CreateUserAsync(new ApplicationUser()
            {
                UserName = "editor",
                FullName = "Editor",
                EmailConfirmed = true
            }, new string[] { FurizaMasterRoles.Editor });
            await CreateUserAsync(new ApplicationUser()
            {
                UserName = "approver",
                FullName = "Approver",
                EmailConfirmed = true
            }, new string[] { FurizaMasterRoles.Approver });
            await CreateUserAsync(new ApplicationUser()
            {
                UserName = "user",
                FullName = "Basic User",
                EmailConfirmed = true
            });

            logger.LogInformation("Database filled.");
        }

        private async Task CreateUserAsync(ApplicationUser user, string[] additionalRoles = null)
        {
            var userEntity = await userManager.FindByNameAsync(user.UserName);
            if (userEntity == null)
            {
                var creationResult = await userManager.CreateAsync(user, user.UserName.ToLower());
                if (creationResult.Succeeded)
                {
                    await userManager.AddClaimAsync(user, new Claim(FurizaClaimNames.SystemUser, ""));

                    logger.LogInformation($"User '{user}' created.");

                    userEntity = await userManager.FindByNameAsync(user.UserName);
                }
                else
                {
                    logger.LogWarning($"An error occurred while creating the user '{user}'.");
                    return;
                }
            }

            if (additionalRoles != null && additionalRoles.Any())
                foreach (var role in additionalRoles)
                    await userManager.AddToRoleAsync(userEntity, role);
            else
                await userManager.AddToRoleAsync(userEntity, FurizaMasterRoles.Viewer);

            logger.LogInformation($"Default roles and claims set to user '{user}'.");
        }
    }
}