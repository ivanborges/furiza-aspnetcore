using Furiza.Audit.Abstractions;
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
        private readonly IAuditTrailProvider auditTrailProvider;

        public FurizaUserStore(IUserPrincipalBuilder userPrincipalBuilder,
            IAuditTrailProvider auditTrailProvider,
            ApplicationDbContext context, 
            IdentityErrorDescriber describer = null) : base(context, describer)
        {
            this.userPrincipalBuilder = userPrincipalBuilder ?? throw new ArgumentNullException(nameof(userPrincipalBuilder));
            this.auditTrailProvider = auditTrailProvider ?? throw new ArgumentNullException(nameof(auditTrailProvider));
        }

        public async override Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // melhorar esquema abaixo... ideal é simplesmente pegar do userprincipalbuilder...
            var creationUser = userPrincipalBuilder?.UserPrincipal?.UserName
                ?? (user.Company == "furiza"
                ? "superuser"
                : throw new UnauthorizedAccessException());

            Context.Set<ApplicationUser>().Add(user);

            await SaveChanges(cancellationToken);

            await auditTrailProvider.AddTrailsAsync(AuditOperation.Create, creationUser, new AuditableObjects<ApplicationUser>(user.Id.ToString(), user));

            return IdentityResult.Success;
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
                ClientId = new Guid(clientId)
            });

            // TODO: por qu nao tem o SaveChanges aqui ?
        }
    }
}