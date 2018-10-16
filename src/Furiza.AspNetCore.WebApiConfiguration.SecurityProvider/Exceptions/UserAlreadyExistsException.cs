using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class UserAlreadyExistsException : CoreException
    {
        public override string Key => "UserAlreadyExists";
        public override string Message => "Username already exists.";
    }
}