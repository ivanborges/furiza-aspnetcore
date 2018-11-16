﻿using Furiza.Audit.Abstractions;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
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

            Context.Set<ApplicationUser>().Add(user);

            await SaveChanges(cancellationToken);

            if (!user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Create, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUser>(user.Id.ToString(), user));

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

            var clientId = !user.IsSystemUser
                ? userPrincipalBuilder.GetCurrentClientId()
                : default(Guid?);

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken) ?? 
                throw new InvalidOperationException($"Invalid role name [{normalizedRoleName}] to assign to user.");

            var roleAssigment = new ApplicationUserRole()
            {
                UserId = user.Id,
                RoleId = roleEntity.Id,
                ClientId = clientId
            };

            Context.Set<ApplicationUserRole>().Add(roleAssigment);

            if (!user.IsSystemUser)
                await auditTrailProvider.AddTrailsAsync(AuditOperation.Create, userPrincipalBuilder.UserPrincipal.UserName, new AuditableObjects<ApplicationUserRole>($"{roleAssigment.UserId}.{roleAssigment.RoleId}", roleAssigment));
        }

        public async override Task RemoveFromRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: implemenbtar
        }
    }
}