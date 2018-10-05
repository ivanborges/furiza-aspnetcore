using Furiza.AspNetCore.ExceptionHandling;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFurizaExceptionHandling(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            applicationBuilder.UseMiddleware<ExceptionMiddleware>();

            return applicationBuilder;
        }
    }
}