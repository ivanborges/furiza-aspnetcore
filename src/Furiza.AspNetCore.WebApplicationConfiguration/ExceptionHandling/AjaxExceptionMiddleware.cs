using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.ExceptionHandling
{
    public class AjaxExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public AjaxExceptionMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                if (context.Request.IsAjaxRequest())
                {
                    context.Response.StatusCode = e is HttpRequestException
                        ? (int)HttpStatusCode.ServiceUnavailable
                        : (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { e.Message }, new GeneralJsonSerializerSettings()));
                }
                else
                    throw;
            }
        }
    }
}