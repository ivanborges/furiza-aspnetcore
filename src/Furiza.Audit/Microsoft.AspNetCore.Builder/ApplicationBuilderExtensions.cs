using Furiza.Audit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder RunFurizaAuditInitializer(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var auditConfiguration = applicationBuilder.ApplicationServices.GetService<AuditConfiguration>();
            switch (auditConfiguration.Tool.Value)
            {
                case AuditTool.SqlServerAndDapper:
                    applicationBuilder.RunFurizaAuditSqlServerDapperInitializer();
                    break;
            }

            return applicationBuilder;
        }
    }
}