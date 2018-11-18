using Furiza.AspNetCore.WebApiConfiguration;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1
{
    public class Startup : RootStartup
    {
        protected override ApiProfile ApiProfile => new ApiProfile()
        {
            Name = "WebApplication1 client",
            Description = "Api de Teste Carai!",
            DefaultVersion = "1.0"
        };

        public Startup(IConfiguration configuration) : base(configuration)
        {
        }
        
        protected override void AddCustomServicesAtTheEnd(IServiceCollection services)
        {
            services.AddTransient<IScopedRoleAssignmentProvider, ScopedRoleAssignmentProviderTeste>();
        }

        protected override void AddCustomServicesAtTheBeginning(IServiceCollection services)
        {

        }

        protected override void AddCustomMiddlewaresToTheBeginningOfThePipeline(IApplicationBuilder app)
        {

        }

        protected override void AddCustomMiddlewaresToTheEndOfThePipeline(IApplicationBuilder app)
        {

        }
    }
}