using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    public class IdentityOperationExceptionItem : CoreExceptionItem
    {
        public IdentityOperationExceptionItem(string code, string description) : base(code, description)
        {
        }
    }
}