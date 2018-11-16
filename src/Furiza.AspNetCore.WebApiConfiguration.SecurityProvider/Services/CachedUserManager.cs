using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.Base.Core.Identity.Abstractions;
using Furiza.Caching.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    internal class CachedUserManager : ICachedUserManager
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICacheHandler cacheHandler;

        public CachedUserManager(UserManager<ApplicationUser> userManager,
            ICacheHandler cacheHandler)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.cacheHandler = cacheHandler ?? throw new ArgumentNullException(nameof(cacheHandler));
        }

        public async Task<ApplicationUser> GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(string username, Guid clientId)
        {
            var normalizedUserName = username.ToUpper().Trim();
            if (!cacheHandler.TryGetValue<ApplicationUser>(normalizedUserName, out var user))
            {
                user = await userManager.Users
                    .Include(u => u.IdentityUserRoles)
                        .ThenInclude(ur => ur.IdentityRole)
                    .Include(u => u.IdentityClaims)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);

                if (user != null && user.EmailConfirmed && user.RoleAssignments.Any())
                    await cacheHandler.SetAsync(normalizedUserName, user, new string[] { nameof(ApplicationUser.Claims), nameof(ApplicationUser.RoleAssignments), nameof(ApplicationUser.IsSystemUser) });
            }

            if (user != null)
            {
                user.IdentityUserRoles = user.IdentityUserRoles.Where(ur => ur.ClientId == clientId).ToList();
                user.IdentityClaims.Add(new ApplicationUserClaim()
                {
                    ClaimType = FurizaClaimNames.ClientId,
                    ClaimValue = clientId.ToString()
                });
            }

            return user;
        }

        public async Task RemoveUserByUserNameAsync(string username) => await cacheHandler.RemoveAsync<ApplicationUser>(username.ToUpper().Trim());
    }
}