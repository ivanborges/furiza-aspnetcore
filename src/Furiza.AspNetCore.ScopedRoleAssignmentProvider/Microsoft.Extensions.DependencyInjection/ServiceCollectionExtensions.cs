using Furiza.AspNetCore.ScopedRoleAssignmentProvider;
using Furiza.Base.Core.Identity.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaScopedRoleAssignmentProvider(this IServiceCollection services, ScopedRoleAssignmentProviderConfiguration scopedRoleAssignmentProviderConfiguration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(scopedRoleAssignmentProviderConfiguration ?? throw new ArgumentNullException(nameof(scopedRoleAssignmentProviderConfiguration)));

            services.AddTransient<IScopedRoleAssignmentProvider, FurizaScopedRoleAssignmentProvider>();

            return services;
        }
    }
}