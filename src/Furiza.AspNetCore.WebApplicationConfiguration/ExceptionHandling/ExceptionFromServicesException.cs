using Furiza.Base.Core.Exceptions.Serialization;
using System;
using System.Net;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApplicationConfiguration.ExceptionHandling
{
    [Serializable]
    public class ExceptionFromServicesException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public BadRequestError BadRequestError { get; set; }
        public InternalServerError InternalServerError { get; set; }

        public ExceptionFromServicesException(HttpStatusCode httpStatusCode) : base()
        {
            HttpStatusCode = httpStatusCode;
        }

        public ExceptionFromServicesException(HttpStatusCode httpStatusCode, BadRequestError badRequestError) : this(httpStatusCode)
        {
            BadRequestError = badRequestError;
        }

        public ExceptionFromServicesException(HttpStatusCode httpStatusCode, InternalServerError internalServerError) : this(httpStatusCode)
        {
            InternalServerError = internalServerError;
        }

        protected ExceptionFromServicesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}