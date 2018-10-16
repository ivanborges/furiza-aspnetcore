using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public class ModelValidationExceptionItem : CoreExceptionItem
    {
        public ModelValidationExceptionItem(string message) : base("ModelValidationItem", message)
        {
        }
    }
}