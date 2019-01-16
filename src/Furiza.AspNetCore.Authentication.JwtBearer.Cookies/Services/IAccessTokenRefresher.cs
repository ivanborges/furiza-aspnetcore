using System;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Cookies.Services
{
    public interface IAccessTokenRefresher
    {
        Task<RefreshTokenResult> RefreshAsync(Guid clientId, string refreshToken);
    }

    public class RefreshTokenResult
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
    }
}