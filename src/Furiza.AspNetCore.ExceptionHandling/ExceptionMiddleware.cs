using Furiza.Base.Core.Exceptions;
using Furiza.Base.Core.Exceptions.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ExceptionMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            logger = loggerFactory?.CreateLogger<ExceptionMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var statusCode = GetHttpStatusCodeFromExceptionType(ex);
                object result;

                if (ex is CoreException)
                {
                    logger.LogInformation(ex, ex.Message);

                    result = new BadRequestError(ex as CoreException);
                }
                else
                {
                    var internalServerError = new InternalServerError(ex);

                    if (ex is AggregateException)
                        foreach (var inner in (ex as AggregateException).InnerExceptions)
                            logger.LogError(inner, $"An aggregate internal error occurred [LogId:{internalServerError.LogId}].", internalServerError.LogId);
                    else
                        logger.LogError(ex, $"An internal error occurred [LogId:{internalServerError.LogId}].", internalServerError.LogId);

                    result = internalServerError;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(result, new GeneralJsonSerializerSettings()));
            }
        }

        private static HttpStatusCode GetHttpStatusCodeFromExceptionType(Exception ex)
        {
            HttpStatusCode statusCode;

            if (ex is ModelValidationException)
                statusCode = HttpStatusCode.NotAcceptable;
            else if (ex is ResourceNotFoundException)
                statusCode = HttpStatusCode.NotFound;
            else if (ex is CoreException)
                statusCode = HttpStatusCode.BadRequest;
            else
                statusCode = HttpStatusCode.InternalServerError;

            return statusCode;
        }
    }
}