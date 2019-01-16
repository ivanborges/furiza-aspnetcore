using Furiza.AspNetCore.Authentication.JwtBearer.Cookies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaCookieAuthentication(this IServiceCollection services, AuthenticationConfiguration authenticationConfiguration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(authenticationConfiguration ?? throw new ArgumentNullException(nameof(authenticationConfiguration)));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddCookie(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    if (!string.IsNullOrWhiteSpace(authenticationConfiguration.CookieName))
                        options.Cookie.Name = authenticationConfiguration.CookieName;

                    if (!string.IsNullOrWhiteSpace(authenticationConfiguration.LoginPath))
                        options.LoginPath = authenticationConfiguration.LoginPath;

                    if (!string.IsNullOrWhiteSpace(authenticationConfiguration.AccessDeniedPath))
                        options.AccessDeniedPath = authenticationConfiguration.AccessDeniedPath;

                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = TokenMonitor.ValidateAccessTokenAsync
                    };
                });

            services.AddScoped<CookiesManager>();

            return services;
        }
    }
}