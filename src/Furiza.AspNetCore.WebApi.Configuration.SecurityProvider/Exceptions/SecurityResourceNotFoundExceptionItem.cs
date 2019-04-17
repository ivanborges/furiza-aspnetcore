using Furiza.AspNetCore.ExceptionHandling;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    public class SecurityResourceNotFoundExceptionItem : ResourceNotFoundExceptionItem
    {
        public static SecurityResourceNotFoundExceptionItem User => new SecurityResourceNotFoundExceptionItem("User", "User does not exist.");
        public static SecurityResourceNotFoundExceptionItem Role => new SecurityResourceNotFoundExceptionItem("Role", "Role does not exist.");

        private SecurityResourceNotFoundExceptionItem(string key, string message) : base(key, message)
        {
        }
    }
}