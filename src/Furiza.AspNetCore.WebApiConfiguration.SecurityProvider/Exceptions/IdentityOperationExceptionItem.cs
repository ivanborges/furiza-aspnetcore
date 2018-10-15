using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class IdentityOperationExceptionItem : CoreExceptionItem
    {
        public IdentityOperationExceptionItem(string code, string description) : base(code, description)
        {
        }
    }
}