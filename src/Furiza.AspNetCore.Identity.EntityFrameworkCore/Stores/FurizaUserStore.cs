using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore.Stores
{
    internal class FurizaUserStore : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid, ApplicationUserClaim, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>
    {
        private readonly IUserPrincipalBuilder userPrincipalBuilder;

        public FurizaUserStore(IUserPrincipalBuilder userPrincipalBuilder,
            ApplicationDbContext context, 
            IdentityErrorDescriber describer = null) : base(context, describer)
        {
            this.userPrincipalBuilder = userPrincipalBuilder ?? throw new ArgumentNullException(nameof(userPrincipalBuilder));
        }

        public async override Task AddToRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
                throw new ArgumentNullException(nameof(normalizedRoleName));

            // testar se vai conseguir obter na inicialização da primeira api ne...
            var clientId = userPrincipalBuilder?.UserPrincipal?.Claims?.SingleOrDefault(c => c.Type == FurizaClaimNames.ClientId)?.Value;
            if (string.IsNullOrWhiteSpace(clientId) || clientId == default(Guid).ToString())
                throw new UnauthorizedAccessException();

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity == null)
                throw new InvalidOperationException($"Invalid role name [{normalizedRoleName}] to assign to user.");

            Context.Set<ApplicationUserRole>().Add(new ApplicationUserRole()
            {
                UserId = user.Id,
                RoleId = roleEntity.Id,
                ClientId = new Guid(clientId),
                CreationDate = DateTime.UtcNow,
                CreationUser = userPrincipalBuilder.UserPrincipal.UserName
            });
        }
    }
}