using Furiza.Base.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.ExceptionHandling
{
    internal class ExceptionHandler
    {
        public static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        {
            var statusCode = null as HttpStatusCode?;
            var result = null as object;

            if (exception is CoreException)
            {
                logger.LogInformation(exception, exception.Message);

                statusCode = exception is ModelValidationException
                    ? HttpStatusCode.NotAcceptable
                    : HttpStatusCode.BadRequest;
                result = exception;
            }
            else
            {
                var internalServerError = null as InternalServerError;
                if (exception is AggregateException)
                {
                    var agEx = exception as AggregateException;
                    internalServerError = new InternalServerError(string.Join(" | ", agEx.InnerExceptions.Select(e => e.Message)));
                    foreach (var inner in agEx.InnerExceptions)
                        logger.LogError(inner, $"An aggregate internal error occurred [LogId:{internalServerError.LogId}].", internalServerError.LogId);
                }
                else
                {
                    internalServerError = new InternalServerError(exception.Message);
                    logger.LogError(exception, $"An internal error occurred [LogId:{internalServerError.LogId}].", internalServerError.LogId);
                }

                statusCode = HttpStatusCode.InternalServerError;
                result = internalServerError;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(result, new CoreExceptionJsonSerializerSettings()));
        }
    }
}