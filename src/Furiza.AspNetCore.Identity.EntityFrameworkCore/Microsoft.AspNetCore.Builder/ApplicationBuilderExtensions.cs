using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFurizaIdentityMigration(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<ApplicationDbContext>();

                logger.LogInformation("Applying migrations for Identity...");

                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                    context.Database.Migrate();

                logger.LogInformation("Migrations applied.");
            }

            return applicationBuilder;
        }

        public static IApplicationBuilder UseFurizaIdentityInitializer(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<IdentityInitializer>();

                logger.LogInformation("Filling database with system users and roles...");

                serviceScope.ServiceProvider.GetService<IdentityInitializer>().Initialize();

                logger.LogInformation("Database filled.");
            }

            return applicationBuilder;
        }
    }
}