using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.Caching.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ApplicationUser> GetUserByUserNameAsync(string username)
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

                if (user != null && user.EmailConfirmed && user.Roles.Any())
                    await cacheHandler.SetAsync(normalizedUserName, user);
            }

            return user;
        }
    }
}