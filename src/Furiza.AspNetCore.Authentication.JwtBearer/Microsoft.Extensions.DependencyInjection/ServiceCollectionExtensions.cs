using Furiza.AspNetCore.Authentication.JwtBearer;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaJwtAuthentication(this IServiceCollection services, JwtConfiguration jwtConfiguration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration)));

            var signingConfiguration = new SigningConfiguration();
            services.AddSingleton(signingConfiguration);

            services.AddScoped(typeof(IUserTokenizer<>), typeof(UserTokenizer<>));
            services.AddScoped(typeof(IUserContext<,,>), typeof(UserContext<,,>));

            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>();

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                bearerOptions.RequireHttpsMetadata = false;
                bearerOptions.SaveToken = true;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtConfiguration.Issuer,
                    ValidAudience = jwtConfiguration.Audience,
                    IssuerSigningKey = signingConfiguration.Key,

                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}