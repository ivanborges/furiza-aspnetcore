using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaim, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TODO: renomear tabelas aspnet => furiza ? 
            // por que nao criar a entidade ScopedRoleAssign aqui e usar este proprio DbContext ??? 

            builder.Entity<ApplicationUser>(user =>
            {
                user.Ignore(u => u.Claims);
                user.Ignore(u => u.RoleAssignments);
                user.Ignore(u => u.IsSystemUser);
            });
            builder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.Ignore(ur => ur.UserName);
                userRole.Ignore(ur => ur.Role);

                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.IdentityRole)
                    .WithMany(r => r.IdentityUserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.IdentityUser)
                    .WithMany(u => u.IdentityUserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            builder.Entity<ApplicationUserClaim>(userClaim =>
            {
                userClaim.HasOne(uc => uc.IdentityUser)
                    .WithMany(u => u.IdentityClaims)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();
            });
        }
    }
}