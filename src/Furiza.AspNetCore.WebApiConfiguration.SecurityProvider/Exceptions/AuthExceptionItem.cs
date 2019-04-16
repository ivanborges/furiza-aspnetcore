using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Auth;
using Furiza.Base.Core.Exceptions;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class AuthExceptionItem : CoreExceptionItem
    {
        public static AuthExceptionItem UserLockedOut => new AuthExceptionItem("UserLockedOut", "User is locked out.");
        public static AuthExceptionItem UserRequired => new AuthExceptionItem("UserRequired", $"The {nameof(AuthPost.User)} field is required.");
        public static AuthExceptionItem PasswordRequired => new AuthExceptionItem("PasswordRequired", $"The {nameof(AuthPost.Password)} field is required.");
        public static AuthExceptionItem RefreshTokenRequired => new AuthExceptionItem("RefreshTokenRequired", $"The {nameof(AuthPost.RefreshToken)} field is required.");
        public static AuthExceptionItem EmailConfirmationRequired => new AuthExceptionItem("EmailConfirmationRequired", "Email confirmation is required in order to authenticate.");
        public static AuthExceptionItem RoleAssignmentRequired => new AuthExceptionItem("RoleAssignmentRequired", "Some role assignment in the specified client is required in order to authenticate.");

        private AuthExceptionItem(string key, string message) : base(key, message)
        {
        }
    }
}