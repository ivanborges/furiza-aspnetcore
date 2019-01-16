using Furiza.AspNetCore.Authentication.JwtBearer.Cookies.Services;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Cookies
{
    internal static class TokenMonitor
    {
        public static async Task ValidateAccessTokenAsync(CookieValidatePrincipalContext context)
        {
            var userPrincipalBuilder = context.HttpContext.RequestServices.GetService<IUserPrincipalBuilder>();

            var issuedClaim = userPrincipalBuilder.UserPrincipal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
            var issuedAt = issuedClaim.ToInt64().ToUnixEpochDate();

            var expiresClaim = userPrincipalBuilder.UserPrincipal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            var expiresAt = expiresClaim.ToInt64().ToUnixEpochDate();

            // Calculate how many minutes the token is valid. ###########
            var validWindow = (expiresAt - issuedAt).TotalMinutes;

            // Refresh token half way the expiration. ###################
            var refreshDateTime = issuedAt.AddMinutes(0.5 * validWindow);

            // Refresh JWT Token if needed. #############################
            if (DateTime.UtcNow > refreshDateTime)
            {
                var accessTokenRefresher = context.HttpContext.RequestServices.GetService<IAccessTokenRefresher>();
                var refreshTokenResult = await accessTokenRefresher.RefreshAsync(userPrincipalBuilder.GetCurrentClientId(), userPrincipalBuilder.UserPrincipal.Claims.FirstOrDefault(c => c.Type == FurizaClaimNames.RefreshToken)?.Value);                

                var cookiesManager = context.HttpContext.RequestServices.GetService<CookiesManager>();
                await cookiesManager.CreateCookieAsync(refreshTokenResult.AccessToken, refreshTokenResult.RefreshToken);
            }
        }
    }
}