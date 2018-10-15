using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1
{
    public class Startup : SecurityRootStartup
    {
        protected override ApiProfile ApiProfile => new ApiProfile()
        {
            Name = "WebApplication2",
            Description = "Api de Teste Carai!",
            DefaultVersion = "1.0"
        };

        public Startup(IConfiguration configuration) : base(configuration)
        {
        }
        
        protected override void AddCustomServicesAtTheEnd(IServiceCollection services)
        {
            

            services.AddTransient<ISignInManager<ApplicationUser>, SignInTeste>();
            services.AddTransient<IPasswordGenerator, PasswordGeneratorTeste>();
            services.AddTransient<IUserNotifier, EmailSenderTeste>();

            //services.AddAutoMapper(typeof(SecurityRootStartup).Assembly);
        }

        protected override void AddCustomMiddlewaresToTheBeginningOfThePipeline(IApplicationBuilder app)
        {
            
        }

        protected override void AddCustomMiddlewaresToTheEndOfThePipeline(IApplicationBuilder app)
        {
            base.AddCustomMiddlewaresToTheEndOfThePipeline(app);

            //mais qq coisa...        
        }
    }
}