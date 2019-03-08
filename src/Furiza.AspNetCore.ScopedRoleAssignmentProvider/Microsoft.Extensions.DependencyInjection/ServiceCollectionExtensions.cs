using Furiza.AspNetCore.ScopedRoleAssignmentProvider;
using Furiza.AspNetCore.ScopedRoleAssignmentProvider.RestClients;
using Furiza.Base.Core.Identity.Abstractions;
using Refit;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFurizaScopedRoleAssignmentProvider(this IServiceCollection services, ScopedRoleAssignmentProviderConfiguration scopedRoleAssignmentProviderConfiguration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(scopedRoleAssignmentProviderConfiguration ?? throw new ArgumentNullException(nameof(scopedRoleAssignmentProviderConfiguration)));

            services.AddTransient(serviceProvider =>
            {
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(scopedRoleAssignmentProviderConfiguration.SecurityProviderApiUrl)
                };

                return RestService.For<ISecurityProviderClient>(httpClient);
            });

            services.AddTransient<IScopedRoleAssignmentProvider, FurizaScopedRoleAssignmentProvider>();

            return services;
        }
    }
}