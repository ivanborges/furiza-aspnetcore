﻿using AutoMapper;
using Furiza.AspNetCore.Authentication.JwtBearer;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.ScopedRoleAssignmentProvider;
using Furiza.Base.Core.Identity.Abstractions;
using Furiza.Caching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Furiza.AspNetCore.WebApi.Configuration
{
    public abstract class RootStartup
    {
        private string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = PlatformServices.Default.Application.ApplicationName + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }

        protected ICollection<Assembly> AutomapperAssemblies { get; } = new List<Assembly>();
        protected abstract ApiProfile ApiProfile { get; }
        protected IConfiguration Configuration { get; }

        protected RootStartup(IConfiguration configuration) =>
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFurizaLogging(Configuration);

            AddCustomServicesAtTheBeginning(services);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(ApiProfile.DefaultCultureInfo);
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo(ApiProfile.DefaultCultureInfo) };
                options.RequestCultureProviders.Clear();
            });

            services.AddFurizaJwtAuthentication(Configuration.TryGet<JwtConfiguration>());
            services.AddFurizaCaching(Configuration.TryGet<CacheConfiguration>());
            services.AddFurizaAudit(Configuration, ApiProfile.Name);
            services.AddFurizaScopedRoleAssignmentProvider(Configuration.TryGet<ScopedRoleAssignmentProviderConfiguration>());
            services.AddMvc(AddMvcOptions).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAuthorization(AddAuthorizationOptions);
            services.AddHttpContextAccessor();
            services.Configure<ApiBehaviorOptions>(AddApiBehaviorOptions);
            AddSwaggerWithApiVersioning(services);

            AddCustomServicesAtTheEnd(services);

            if (AutomapperAssemblies.Any())
                services.AddAutoMapper(AutomapperAssemblies);
        }
        
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            app.UseRequestLocalization();
            app.UseFurizaExceptionHandling();
            app.UseFurizaAuditIpAddressRetriever();

            AddCustomMiddlewaresToTheBeginningOfThePipeline(app);

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Where(a => !a.IsDeprecated).OrderBy(a => a.ApiVersion.MajorVersion).ThenBy(a => a.ApiVersion.MinorVersion))
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"{ApiProfile.Name} {description.GroupName}{(!description.GroupName.Contains(".") ? ".0" : "")}");
            });

            app.RunFurizaAuditInitializer();

            AddCustomMiddlewaresToTheEndOfThePipeline(app);
        }

        #region [+] Virtual
        protected virtual void AddMvcOptions(MvcOptions options)
        {
            options.Filters.Add(typeof(ModelValidationAttribute));
        }

        protected virtual void AddAuthorizationOptions(AuthorizationOptions options)
        {
            options.AddPolicy(FurizaPolicies.RequireAdministratorRights, policy => policy.RequireRole(FurizaMasterRoles.Superuser, FurizaMasterRoles.Administrator));
            options.AddPolicy(FurizaPolicies.RequireEditorRights, policy => policy.RequireRole(FurizaMasterRoles.Superuser, FurizaMasterRoles.Administrator, FurizaMasterRoles.Editor));
            options.AddPolicy(FurizaPolicies.RequireApproverRights, policy => policy.RequireRole(FurizaMasterRoles.Superuser, FurizaMasterRoles.Administrator, FurizaMasterRoles.Editor, FurizaMasterRoles.Approver));
        }

        protected virtual void AddApiBehaviorOptions(ApiBehaviorOptions options)
        {
            //options.SuppressConsumesConstraintForFormFileParameters = true;
            //options.SuppressInferBindingSourcesForParameters = true;
            options.SuppressModelStateInvalidFilter = true;
        }
        #endregion

        #region [+] Abstract
        protected abstract void AddCustomServicesAtTheBeginning(IServiceCollection services);
        protected abstract void AddCustomServicesAtTheEnd(IServiceCollection services);
        protected abstract void AddCustomMiddlewaresToTheBeginningOfThePipeline(IApplicationBuilder app);
        protected abstract void AddCustomMiddlewaresToTheEndOfThePipeline(IApplicationBuilder app); 
        #endregion

        #region [+] Privates
        private void AddSwaggerWithApiVersioning(IServiceCollection services)
        {
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersion.Parse(ApiProfile.DefaultVersion);
            });
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    In = "header",
                    Description = "Please enter your JWT with the prefix Bearer into the field below.",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });

                var apiVersionDescriptionProvider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Where(a => !a.IsDeprecated).OrderBy(a => a.ApiVersion.MajorVersion).ThenBy(a => a.ApiVersion.MinorVersion))
                    options.SwaggerDoc(description.GroupName, CreateSwaggerInfoForApiVersion(description));

                options.DescribeAllEnumsAsStrings();
                options.OperationFilter<SwaggerDefaultValues>();
                options.IncludeXmlComments(XmlCommentsFilePath);
            });
        }

        private Info CreateSwaggerInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info()
            {
                Title = ApiProfile.Name,
                Description = ApiProfile.Description,
                Version = description.ApiVersion.ToString()
            };

            if (info.Version.Equals("1.0"))
                info.Description += "<br>Initial version.";
            else
            {
                var versionDescription = ApiProfile.VersioningDescriptions?[info.Version];
                if (!string.IsNullOrWhiteSpace(versionDescription))
                    info.Description += $"<br>{versionDescription}";
            }

            if (description.IsDeprecated)
                info.Description += "<br><br><span style=\"color: #ff0000;font-weight: bold;\">This version is already deprecated.</span>";

            return info;
        }
        #endregion
    }
}