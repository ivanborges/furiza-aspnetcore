using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.Identity.EntityFrameworkCore.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaIdentity(this IServiceCollection services, IdentityConfiguration identityConfiguration, Action<IdentityOptions> identityOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration)));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(identityConfiguration.ConnectionString));

            services.AddIdentity<ApplicationUser, ApplicationRole>(identityOptions)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IUserStore<ApplicationUser>, FurizaUserStore>();

            services.AddScoped<IdentityInitializer>();

            return services;
        }
    }
}