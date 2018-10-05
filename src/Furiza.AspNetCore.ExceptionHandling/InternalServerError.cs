using System;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public class InternalServerError
    {
        public Guid LogId { get; set; }
        public string Message { get; set; }

        public InternalServerError()
        {

        }

        public InternalServerError(string message)
        {
            LogId = Guid.NewGuid();
            Message = message;
        }
    }
}