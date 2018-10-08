using Furiza.AspNetCore.Authentication.JwtBearer;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Furiza.AspNetCore.WebApiConfiguration
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

        protected abstract ApiProfile ApiProfile { get; }
        protected IConfiguration Configuration { get; }

        public RootStartup(IConfiguration configuration) =>
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFurizaLogging(Configuration, ApiProfile.Name);
            services.AddFurizaIdentity(Configuration.TryGet<IdentityConfiguration>());
            services.AddFurizaJwtAuthentication(Configuration.TryGet<JwtConfiguration>());
            services.AddFurizaCaching(Configuration.TryGet<CacheConfiguration>());
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ModelValidationAttribute));
                AddCustomFilters(options);
            });

            AddSwaggerWithApiVersioning(services);
            AddCustomServices(services);
        }

        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            app.UseFurizaExceptionHandling();

            AddCustomMiddlewaresToTheBeginningOfThePipeline(app);

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.ToList().Where(a => !a.IsDeprecated).OrderBy(a => a.ApiVersion.MajorVersion).ThenBy(a => a.ApiVersion.MinorVersion))
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"{ApiProfile.Name} {description.GroupName}{(!description.GroupName.Contains(".") ? ".0" : "")}");
            });

            AddCustomMiddlewaresToTheEndOfThePipeline(app);
        }

        protected abstract void AddCustomFilters(MvcOptions options);
        protected abstract void AddCustomServices(IServiceCollection services);
        protected abstract void AddCustomMiddlewaresToTheBeginningOfThePipeline(IApplicationBuilder app);
        protected abstract void AddCustomMiddlewaresToTheEndOfThePipeline(IApplicationBuilder app);

        #region [+] Privates
        private void AddSwaggerWithApiVersioning(IServiceCollection services)
        {
            services.AddMvcCore().AddVersionedApiExplorer(options =>
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
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });

                var apiVersionDescriptionProvider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.ToList().Where(a => !a.IsDeprecated).OrderBy(a => a.ApiVersion.MajorVersion).ThenBy(a => a.ApiVersion.MinorVersion))
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