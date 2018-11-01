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
            this.userManager = userManager ?? throw new System.ArgumentNullException(nameof(userManager));
            this.cacheHandler = cacheHandler ?? throw new System.ArgumentNullException(nameof(cacheHandler));
        }

        public async Task<ApplicationUser> GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(string username, Guid? clientId = null)
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
                    await cacheHandler.SetAsync(normalizedUserName, user);
            }

            if (clientId.HasValue && clientId.Value != default(Guid))
            {
                user.IdentityUserRoles = user.IdentityUserRoles.Where(ur => ur.ClientId == clientId.Value).ToList();
                user.IdentityClaims.Add(new ApplicationUserClaim()
                {
                    ClaimType = FurizaClaimNames.ClientId,
                    ClaimValue = clientId.Value.ToString()
                });
            }

            return user;
        }
    }
}