using Furiza.AspNetCore.Authentication.JwtBearer.Cookies.Services;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Cookies
{
    internal static class TokenMonitor
    {
        public static async Task ValidateAccessTokenAsync(CookieValidatePrincipalContext context)
        {
            if (!context.Principal.Claims.Any())
                return;

            var issuedClaim = context.Principal.FindFirst(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
            var issuedAt = issuedClaim.ToInt64().ToUnixEpochDate();

            var expiresClaim = context.Principal.FindFirst(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            var expiresAt = expiresClaim.ToInt64().ToUnixEpochDate();

            // Calculate how many minutes the token is valid. ###########
            var validWindow = (expiresAt - issuedAt).TotalMinutes;

            // Refresh token at 90% way the expiration. #################
            var refreshDateTime = issuedAt.AddMinutes(0.9 * validWindow);

            // Refresh JWT Token if needed. #############################
            if (DateTime.UtcNow > refreshDateTime)
            {
                // Generate new access token from refresh token. ########
                var accessTokenRefresher = context.HttpContext.RequestServices.GetService<IAccessTokenRefresher>();
                var refreshTokenResult = await accessTokenRefresher.RefreshAsync(
                    new Guid(context.Principal.FindFirst(c => c.Type == JwtRegisteredClaimNames.Aud).Value),
                    context.Principal.FindFirst(c => c.Type == FurizaClaimNames.RefreshToken).Value);                

                // Create new cookies for new access token. #############
                var cookiesManager = context.HttpContext.RequestServices.GetService<CookiesManager>();
                await cookiesManager.CreateCookieAsync(refreshTokenResult.AccessToken, refreshTokenResult.RefreshToken);

                // Update Identity with new tokens. #####################
                var identity = context.Principal.Identities.FirstOrDefault();
                var accessTokenClaim = identity.FindFirst(c => c.Type == FurizaClaimNames.AccessToken);
                var refreshTokenClaim = identity.FindFirst(c => c.Type == FurizaClaimNames.RefreshToken);

                if (accessTokenClaim != null)
                    identity.RemoveClaim(accessTokenClaim);

                if (refreshTokenClaim != null)
                    identity.RemoveClaim(refreshTokenClaim);

                identity.AddClaim(new Claim(FurizaClaimNames.AccessToken, refreshTokenResult.AccessToken));
                identity.AddClaim(new Claim(FurizaClaimNames.RefreshToken, refreshTokenResult.RefreshToken));
                //#######################################################
            }
        }
    }
}