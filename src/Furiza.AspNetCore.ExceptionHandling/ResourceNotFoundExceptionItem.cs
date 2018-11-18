using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public abstract class ResourceNotFoundExceptionItem : CoreExceptionItem
    {
        //public ResourceNotFoundExceptionItem(string message) : base("ResourceNotFoundItem", message)
        //{
        //}

        protected ResourceNotFoundExceptionItem(string key, string message) : base(key, message)
        {
        }
    }
}