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
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}