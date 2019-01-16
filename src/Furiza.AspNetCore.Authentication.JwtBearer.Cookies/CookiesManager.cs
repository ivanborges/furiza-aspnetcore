using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Cookies
{
    public class CookiesManager
    {
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly SecurityTokenHandler securityTokenHandler;
        protected readonly AuthenticationConfiguration authenticationConfiguration;

        public CookiesManager(IHttpContextAccessor httpContextAccessor,
            SecurityTokenHandler securityTokenHandler,
            AuthenticationConfiguration authenticationConfiguration)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.securityTokenHandler = securityTokenHandler ?? throw new ArgumentNullException(nameof(securityTokenHandler));
            this.authenticationConfiguration = authenticationConfiguration ?? throw new ArgumentNullException(nameof(authenticationConfiguration));
        }

        public virtual async Task CreateCookieAsync(string accessToken, string refreshToken = null)
        {
            await DismissCookieAsync();

            //######################################################################################################################
            // Treatment needed when Issuer or Audience are guids as string. This will prevent different inputs prompt invalid data.
            // Example: {F56B5B81-C116-4196-A07C-F1819AB5A8A7} will become f56b5b81-c116-4196-a07c-f1819ab5a8a7.

            var validIssuer = Guid.TryParse(authenticationConfiguration.Jwt.Issuer, out var guidIssuer)
                ? guidIssuer.ToString()
                : authenticationConfiguration.Jwt.Issuer;

            var validAudience = Guid.TryParse(authenticationConfiguration.Jwt.Audience, out var guidAudience)
                ? guidAudience.ToString()
                : authenticationConfiguration.Jwt.Audience;

            //######################################################################################################################

            var claimsPrincipal = securityTokenHandler.ValidateToken(
                accessToken, 
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = validIssuer,
                    ValidAudience = validAudience,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationConfiguration.Jwt.Secret)),

                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                }, 
                out var securityToken);

            var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
            var jwtSecurityToken = securityTokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            //###################################################
            // Search for missed claims, for example claim 'sub'.

            var extraClaims = jwtSecurityToken.Claims.Where(c => !claimsIdentity.Claims.Any(x => x.Type == c.Type)).ToList();
            extraClaims.Add(new Claim(FurizaClaimNames.AccessToken, accessToken));
            extraClaims.Add(new Claim(FurizaClaimNames.RefreshToken, refreshToken));

            if (claimsIdentity.Claims.Any(c => c.Type == ClaimTypes.Role) && extraClaims.Any(c => c.Type.ToLower() == "role"))
                extraClaims.RemoveAll(c => c.Type.ToLower() == "role");

            claimsIdentity.AddClaims(extraClaims);

            //###################################################

            var authenticationProperties = new AuthenticationProperties()
            {
                IssuedUtc =  claimsIdentity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value.ToInt64().ToUnixEpochDate(),
                ExpiresUtc = claimsIdentity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value.ToInt64().ToUnixEpochDate(),
                IsPersistent = true,                
            };

            await httpContextAccessor.HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, claimsPrincipal, authenticationProperties);
        }

        public virtual async Task DismissCookieAsync()
        {
            await httpContextAccessor.HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
        }
    }
}