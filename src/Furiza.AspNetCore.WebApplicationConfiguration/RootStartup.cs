using AutoMapper;
using Furiza.AspNetCore.Authentication.JwtBearer.Cookies;
using Furiza.AspNetCore.Authentication.JwtBearer.Cookies.Services;
using Furiza.AspNetCore.ScopedRoleAssignmentProvider;
using Furiza.AspNetCore.WebApplicationConfiguration.ExceptionHandling;
using Furiza.AspNetCore.WebApplicationConfiguration.RestClients;
using Furiza.AspNetCore.WebApplicationConfiguration.Services;
using Furiza.Networking.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Furiza.AspNetCore.WebApplicationConfiguration
{
    public abstract class RootStartup
    {
        protected abstract ApplicationProfile ApplicationProfile { get; }
        protected IConfiguration Configuration { get; }

        protected RootStartup(IConfiguration configuration) =>
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            AddCustomServicesAtTheBeginning(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var authenticationConfiguration = Configuration.TryGet<AuthenticationConfiguration>();

            services.AddFurizaNetworking();
            services.AddSingleton(ApplicationProfile);
            services.AddScoped<WebApplicationLoginManager>();
            services.AddScoped<IAccessTokenRefresher, AccessTokenRefresher>();
            services.AddScoped(serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                var httpClient = httpClientFactory.Create(authenticationConfiguration.SecurityProviderApiUrl);

                return RestService.For<ISecurityProviderClient>(httpClient);
            });

            services.AddFurizaCookieAuthentication(authenticationConfiguration);
            services.AddFurizaUserPrincipalBuilder();
            services.AddHttpContextAccessor();
            services.AddFurizaScopedRoleAssignmentProvider(new ScopedRoleAssignmentProviderConfiguration() { SecurityProviderApiUrl = authenticationConfiguration.SecurityProviderApiUrl });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            AddCustomServicesAtTheEnd(services);

            services.AddAutoMapper();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(ApplicationProfile.ErrorPage);
                app.UseHsts();
            }
            
            app.UseMiddleware<RefitExceptionMiddleware>();

            AddCustomMiddlewaresToTheBeginningOfThePipeline(app);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

            AddCustomMiddlewaresToTheEndOfThePipeline(app);
        }

        #region [+] Abstract
        protected abstract void AddCustomServicesAtTheBeginning(IServiceCollection services);
        protected abstract void AddCustomServicesAtTheEnd(IServiceCollection services);
        protected abstract void AddCustomMiddlewaresToTheBeginningOfThePipeline(IApplicationBuilder app);
        protected abstract void AddCustomMiddlewaresToTheEndOfThePipeline(IApplicationBuilder app);
        #endregion
    }
}