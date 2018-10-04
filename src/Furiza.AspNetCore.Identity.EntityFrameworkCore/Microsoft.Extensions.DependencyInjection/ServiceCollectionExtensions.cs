using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaIdentity(this IServiceCollection services, IdentityConfiguration identityConfiguration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration)));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(identityConfiguration.ConnectionString));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); // TODO: checar se o erro "Cannot create a DbSet for 'IdentityUserRole<Guid>' because this type is not included in the model for the context" não ocorre... se sim, usar as linhas abaixo.
                //.AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid, ApplicationUserClaim, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>>()
                //.AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid, ApplicationUserRole, IdentityRoleClaim<Guid>>>();

            services.AddSingleton<IdentityInitializer>();

            return services;
        }
    }
}