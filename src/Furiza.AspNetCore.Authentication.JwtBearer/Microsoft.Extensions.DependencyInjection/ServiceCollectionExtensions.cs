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
        public static IServiceCollection AddFurizaJwtAuthentication(this IServiceCollection services, JwtConfiguration jwtConfiguration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration)));

            services.AddScoped(typeof(IUserPrincipalBuilder), typeof(UserPrincipalBuilderTypeless));
            services.AddScoped(typeof(IUserPrincipalBuilder<,>), typeof(UserPrincipalBuilderTyped<,>));

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
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtConfiguration.Audience), // TODO: validar na api de seguranca se o token gerado para um client X serviu para, por exemplo, criar um usuario ! ... para testar, podemos colocar aqui como True fixo... dessa forma, eh proivavrl q receba um unauthorized ao tentar criar user...
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtConfiguration.Issuer,
                    //ValidAudience = jwtConfiguration.Audience,

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