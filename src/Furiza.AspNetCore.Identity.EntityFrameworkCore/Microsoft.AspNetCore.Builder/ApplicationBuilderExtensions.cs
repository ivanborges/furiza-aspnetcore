using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder RunFurizaIdentityMigrations(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<ApplicationDbContext>();

                var identityConfiguration = serviceScope.ServiceProvider.GetService<IdentityConfiguration>();
                if (identityConfiguration.EnableMigrations)
                {
                    logger.LogInformation("Applying migrations for Identity...");

                    using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                        context.Database.Migrate();

                    logger.LogInformation("Identity migrations applied.");
                }
                else
                    logger.LogInformation("Identity migrations disabled.");
            }

            return applicationBuilder;
        }

        public static IApplicationBuilder RunFurizaIdentityInitializer(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                serviceScope.ServiceProvider.GetService<IdentityInitializer>().Initialize();

            return applicationBuilder;
        }
    }
}