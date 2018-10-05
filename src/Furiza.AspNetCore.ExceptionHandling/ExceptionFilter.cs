using Furiza.Base.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger logger;

        public ExceptionFilter(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory?.CreateLogger<ExceptionFilter>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void OnException(ExceptionContext context)
        {
            ExceptionHandler.HandleExceptionAsync(context.HttpContext, context.Exception, logger).Wait();

            //var statusCode = null as HttpStatusCode?;
            //var result = null as object;
            //var jsonSettings = new CoreExceptionJsonSerializerSettings();

            //if (context.Exception is CoreException)
            //{
            //    logger.LogInformation(context.Exception, context.Exception.Message);

            //    statusCode = context.Exception is ModelValidationException
            //        ? HttpStatusCode.NotAcceptable
            //        : HttpStatusCode.BadRequest;
            //    result = context.Exception;
            //}
            //else
            //{
            //    var internalServerError = null as InternalServerError;
            //    if (context.Exception is AggregateException)
            //    {
            //        var agEx = context.Exception as AggregateException;
            //        internalServerError = new InternalServerError(string.Join(" | ", agEx.InnerExceptions.Select(e => e.Message)));
            //        foreach (var inner in agEx.InnerExceptions)
            //            logger.LogError(inner, $"An aggregate internal error occurred [LogId:{internalServerError.LogId}].", internalServerError.LogId);
            //    }
            //    else
            //    {
            //        internalServerError = new InternalServerError(context.Exception.Message);
            //        logger.LogError(context.Exception, $"An internal error occurred [LogId:{internalServerError.LogId}].", internalServerError.LogId);
            //    }

            //    statusCode = HttpStatusCode.InternalServerError;
            //    result = internalServerError;
            //}

            //context.HttpContext.Response.ContentType = "application/json";
            //context.HttpContext.Response.StatusCode = (int)statusCode;
            //context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result, jsonSettings)).Wait();
        }
    }
}