using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public abstract class ResourceNotFoundExceptionItem : CoreExceptionItem
    {
        protected ResourceNotFoundExceptionItem(string key, string message) : base(key, message)
        {
        }
    }
}