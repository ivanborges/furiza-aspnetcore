using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider
{
    public abstract class SecurityRootStartup : RootStartup
    {
        protected SecurityRootStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void AddCustomServicesAtTheBeginning(IServiceCollection services)
        {
            services.AddFurizaIdentity(Configuration.TryGet<IdentityConfiguration>(), AddIdentityOptions);
            services.AddFurizaJwtAuthenticationProvider();

            services.AddScoped<ICachedUserManager, CachedUserManager>();
        }

        protected override void AddCustomMiddlewaresToTheEndOfThePipeline(IApplicationBuilder app)
        {
            app.RunFurizaIdentityMigrations();
            app.RunFurizaIdentityInitializer();
        }

        #region [+] Virtual
        protected virtual void AddIdentityOptions(IdentityOptions options)
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 4;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;

            options.SignIn.RequireConfirmedEmail = true;
        }
        #endregion
    }
}