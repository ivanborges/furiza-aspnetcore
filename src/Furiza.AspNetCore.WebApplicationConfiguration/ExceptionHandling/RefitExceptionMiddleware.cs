using Furiza.Base.Core.Exceptions.Serialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Refit;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.ExceptionHandling
{
    public class RefitExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public RefitExceptionMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException e)
            {
                switch (e.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.NotAcceptable:
                        var e400 = e.GetContentAs<BadRequestError>();

                        if (context.Request.IsAjaxRequest())
                        {
                            context.Response.StatusCode = (int)e.StatusCode;
                            context.Response.ContentType = "application/json";

                            await context.Response.WriteAsync(JsonConvert.SerializeObject(e400, new GeneralJsonSerializerSettings()));
                        }
                        else
                            throw new ExceptionFromServicesException(e.StatusCode, e400);

                        break;
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                        if (context.Request.IsAjaxRequest())
                            context.Response.StatusCode = (int)e.StatusCode;
                        else
                            throw new ExceptionFromServicesException(e.StatusCode);

                        break;
                    case HttpStatusCode.InternalServerError:
                        var e500 = e.GetContentAs<InternalServerError>();

                        if (context.Request.IsAjaxRequest())
                        {
                            context.Response.StatusCode = (int)e.StatusCode;
                            context.Response.ContentType = "application/json";

                            await context.Response.WriteAsync(JsonConvert.SerializeObject(e500, new GeneralJsonSerializerSettings()));
                        }
                        else
                            throw new ExceptionFromServicesException(e.StatusCode, e500);

                        break;
                    default:
                        throw;
                }
            }
        }
    }
}