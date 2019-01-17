using Furiza.AspNetCore.Authentication.JwtBearer;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaUserPrincipalBuilder(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddScoped(typeof(IUserPrincipalBuilder), typeof(UserPrincipalBuilderTypeless));
            services.AddScoped(typeof(IUserPrincipalBuilder<,>), typeof(UserPrincipalBuilderTyped<,>));

            return services;
        }

        public static IServiceCollection AddFurizaJwtAuthentication(this IServiceCollection services, JwtConfiguration jwtConfiguration)
        {
            services.AddFurizaUserPrincipalBuilder();

            services.AddSingleton(jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration)));

            //######################################################################################################################
            // Treatment needed when Issuer or Audience are guids as string. This will prevent different inputs prompt invalid data.
            // Example: {F56B5B81-C116-4196-A07C-F1819AB5A8A7} will become f56b5b81-c116-4196-a07c-f1819ab5a8a7.

            var validIssuer = Guid.TryParse(jwtConfiguration.Issuer, out var guidIssuer)
                ? guidIssuer.ToString()
                : jwtConfiguration.Issuer;

            var validAudience = Guid.TryParse(jwtConfiguration.Audience, out var guidAudience)
                ? guidAudience.ToString()
                : jwtConfiguration.Audience;

            //######################################################################################################################

            services
                .AddAuthentication(authOptions =>
                {
                    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearerOptions =>
                {
                    bearerOptions.RequireHttpsMetadata = false;
                    bearerOptions.SaveToken = true;
                    bearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = !string.IsNullOrWhiteSpace(validAudience),
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = validIssuer,
                        ValidAudience = validAudience,

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.Secret)),

                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

        public static IServiceCollection AddFurizaJwtAuthenticationProvider(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddScoped(typeof(IUserPrincipalTokenizer), typeof(UserPrincipalTokenizer));
            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>();

            return services;
        }
    }
}