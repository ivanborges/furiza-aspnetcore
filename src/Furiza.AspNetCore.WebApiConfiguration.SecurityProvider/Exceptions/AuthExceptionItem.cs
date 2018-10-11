using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Auth;
using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class AuthExceptionItem : CoreExceptionItem
    {
        public static AuthExceptionItem UserRequired = new AuthExceptionItem("ModelValidationItem", $"The {nameof(Post.User)} field is required.");
        public static AuthExceptionItem PasswordRequired = new AuthExceptionItem("ModelValidationItem", $"The {nameof(Post.Password)} field is required.");
        public static AuthExceptionItem RefreshTokenRequired = new AuthExceptionItem("ModelValidationItem", $"The {nameof(Post.RefreshToken)} field is required.");

        private AuthExceptionItem(string key, string message) : base(key, message)
        {
        }
    }
}