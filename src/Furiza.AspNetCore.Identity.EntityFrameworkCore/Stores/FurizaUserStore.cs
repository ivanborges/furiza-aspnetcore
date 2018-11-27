using Furiza.Audit.Abstractions;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            var result = await base.CreateAsync(user, cancellationToken);
            if (result == IdentityResult.Success && !user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Create, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUser>(user.Id.ToString(), user));

            return result;
        }

        public async override Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.UpdateAsync(user, cancellationToken);
            //if (result == IdentityResult.Success && !user.IsSystemUser)
            //    await auditTrailProvider.AddTrailsAsync(AuditOperation.Update, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUser>(user.Id.ToString(), user));

            return result;
        }

        public async override Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.DeleteAsync(user, cancellationToken);
            if (result == IdentityResult.Success && !user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Delete, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUser>(user.Id.ToString(), user));

            return result;
        }

        public async override Task<bool> IsInRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ValidateParameters(user, normalizedRoleName);

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity != null)
            {
                var roleAssignment = await FindUserRoleAsync(user.Id, roleEntity.Id, userPrincipalBuilder.GetCurrentClientId(), cancellationToken);
                return roleAssignment != null;
            }

            return false;
        }

        public async override Task AddToRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ValidateParameters(user, normalizedRoleName);

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken) ?? 
                throw new InvalidOperationException($"Invalid role name [{normalizedRoleName}] to assign to user.");

            var roleAssignment = new ApplicationUserRole()
            {
                UserId = user.Id,
                RoleId = roleEntity.Id,
                ClientId = userPrincipalBuilder.GetCurrentClientId()
            };

            Context.Set<ApplicationUserRole>().Add(roleAssignment);

            if (!user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Create, 
                    userPrincipalBuilder.UserPrincipal.UserName, 
                    new AuditableObjects<ApplicationUserRole>($"{roleAssignment.UserId}.{roleAssignment.RoleId}.{roleAssignment.ClientId}", roleAssignment),
                    new string[] { nameof(ApplicationUserRole.IdentityUser), nameof(ApplicationUserRole.IdentityRole), nameof(ApplicationUserRole.UserName), nameof(ApplicationUserRole.Role) });
        }

        public async override Task RemoveFromRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ValidateParameters(user, normalizedRoleName);

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity != null)
            {
                var roleAssignment = await FindUserRoleAsync(user.Id, roleEntity.Id, userPrincipalBuilder.GetCurrentClientId(), cancellationToken);
                if (roleAssignment != null)
                {
                    Context.Set<ApplicationUserRole>().Remove(roleAssignment);

                    if (!user.IsSystemUser)
                        await auditTrailProvider.AddTrailsAsync(AuditOperation.Delete, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUserRole>($"{roleAssignment.UserId}.{roleAssignment.RoleId}.{roleAssignment.ClientId}", roleAssignment));
                }
            }
        }

        public async override Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ValidateParameters(user, claims);

            var auditableObjs = new AuditableObjects<ApplicationUserClaim>();
            foreach (var claim in claims)
            {
                var matchedClaims = await Context.Set<ApplicationUserClaim>().Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
                if (matchedClaims.Any())
                    continue;

                var userClaim = CreateUserClaim(user, claim);

                Context.Set<ApplicationUserClaim>().Add(userClaim);

                auditableObjs.AddObject(userClaim.Id.ToString(), userClaim);
            }

            if (auditableObjs.Any() && !user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Create,
                    userPrincipalBuilder.UserPrincipal.UserName,
                    auditableObjs,
                    new string[] { nameof(ApplicationUserClaim.IdentityUser) });
        }

        public async override Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ValidateParameters(user, claims);

            var auditableObjs = new AuditableObjects<ApplicationUserClaim>();
            foreach (var claim in claims)
            {
                var matchedClaims = await Context.Set<ApplicationUserClaim>().Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
                foreach (var userClaim in matchedClaims)
                {
                    Context.Set<ApplicationUserClaim>().Remove(userClaim);

                    auditableObjs.AddObject(userClaim.Id.ToString(), userClaim);
                }
            }

            if (auditableObjs.Any() && !user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Delete, userPrincipalBuilder.UserPrincipal.UserName, auditableObjs);
        }

        protected async virtual Task<ApplicationUserRole> FindUserRoleAsync(Guid userId, Guid roleId, Guid clientId, CancellationToken cancellationToken) => 
            await Context.Set<ApplicationUserRole>().FindAsync(new object[] { userId, roleId, clientId }, cancellationToken);

        private static void ValidateParameters(ApplicationUser user, string normalizedRoleName)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
                throw new ArgumentNullException(nameof(normalizedRoleName));
        }

        private static void ValidateParameters(ApplicationUser user, IEnumerable<Claim> claims)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claims == null)
                throw new ArgumentNullException(nameof(claims));
        }
    }
}