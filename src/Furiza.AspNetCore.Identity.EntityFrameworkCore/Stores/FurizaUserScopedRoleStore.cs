using Furiza.Audit.Abstractions;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore.Stores
{
    public class FurizaUserScopedRoleStore
    {
        private readonly IUserPrincipalBuilder userPrincipalBuilder;
        private readonly IAuditTrailProvider auditTrailProvider;
        private readonly ApplicationDbContext applicationDbContext;

        public DbSet<ApplicationUserScopedRole> ScopedRoleAssignments => applicationDbContext.ScopedRoleAssignments;

        public FurizaUserScopedRoleStore(IUserPrincipalBuilder userPrincipalBuilder,
            IAuditTrailProvider auditTrailProvider,
            ApplicationDbContext applicationDbContext)
        {
            this.userPrincipalBuilder = userPrincipalBuilder ?? throw new ArgumentNullException(nameof(userPrincipalBuilder));
            this.auditTrailProvider = auditTrailProvider ?? throw new ArgumentNullException(nameof(auditTrailProvider));
            this.applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }

        public async Task<bool> IsInScopedRoleAsync(string userName, string roleName, string scopeName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidateAndNormalizeParameters(userName, roleName, scopeName, out var normalizedUserName, out var normalizedRoleName, out var normalizedScopeName);

            var scopedRoleAssignment = await FindUserScopedRoleAsync(normalizedUserName, normalizedRoleName, normalizedScopeName, userPrincipalBuilder.GetCurrentClientId(), cancellationToken);
            return scopedRoleAssignment != null;
        }

        public async Task AddToScopedRoleAsync(string userName, string roleName, string scopeName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidateAndNormalizeParameters(userName, roleName, scopeName, out var normalizedUserName, out var normalizedRoleName, out var normalizedScopeName);

            var scopedRoleAssignment = new ApplicationUserScopedRole()
            {
                UserName = normalizedUserName,
                Role = normalizedRoleName,
                Scope = normalizedScopeName,
                ClientId = userPrincipalBuilder.GetCurrentClientId()
            };

            applicationDbContext.Set<ApplicationUserScopedRole>().Add(scopedRoleAssignment);
            await applicationDbContext.SaveChangesAsync();

            await auditTrailProvider.AddTrailsAsync(AuditOperation.Create,
                userPrincipalBuilder.UserPrincipal.UserName,
                new AuditableObjects<ApplicationUserScopedRole>($"{scopedRoleAssignment.UserName}.{scopedRoleAssignment.Role}.{scopedRoleAssignment.Scope}.{scopedRoleAssignment.ClientId}", scopedRoleAssignment));
        }

        public async Task RemoveFromScopedRoleAsync(string userName, string roleName, string scopeName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidateAndNormalizeParameters(userName, roleName, scopeName, out var normalizedUserName, out var normalizedRoleName, out var normalizedScopeName);

            var scopedRoleAssignment = await FindUserScopedRoleAsync(normalizedUserName, normalizedRoleName, normalizedScopeName, userPrincipalBuilder.GetCurrentClientId(), cancellationToken);
            if (scopedRoleAssignment != null)
            {
                applicationDbContext.Set<ApplicationUserScopedRole>().Remove(scopedRoleAssignment);
                await applicationDbContext.SaveChangesAsync();

                await auditTrailProvider.AddTrailsAsync(AuditOperation.Delete, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUserScopedRole>($"{scopedRoleAssignment.UserName}.{scopedRoleAssignment.Role}.{scopedRoleAssignment.Scope}.{scopedRoleAssignment.ClientId}", scopedRoleAssignment));
            }
        }

        protected async virtual Task<ApplicationUserScopedRole> FindUserScopedRoleAsync(string userName, string role, string scope, Guid clientId, CancellationToken cancellationToken) =>
            await applicationDbContext.Set<ApplicationUserScopedRole>().FindAsync(new object[] { userName, role, scope, clientId }, cancellationToken);

        private static void ValidateAndNormalizeParameters(string userName, string roleName, string scopeName, out string normalizedUserName, out string normalizedRoleName, out string normalizedScopeName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            if (string.IsNullOrWhiteSpace(scopeName))
                throw new ArgumentNullException(nameof(scopeName));

            normalizedUserName = userName.ToLower().Trim();
            normalizedRoleName = roleName.ToLower().Trim();
            normalizedScopeName = scopeName.ToLower().Trim();
        }
    }
}