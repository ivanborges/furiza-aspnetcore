using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class EmailConfirmationRequiredException : CoreException
    {
        public override string Key => "EmailConfirmationRequired";
        public override string Message => "Email confirmation is required in order to authenticate.";
    }
}