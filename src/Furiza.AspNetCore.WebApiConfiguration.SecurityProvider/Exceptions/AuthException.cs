using Furiza.Base.Core.Exceptions;
using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class AuthException : CoreException<AuthExceptionItem>
    {
        public override string Key => "Auth";

        public AuthException(IEnumerable<AuthExceptionItem> coreExceptionItems) : base(coreExceptionItems)
        {
        }
    }
}