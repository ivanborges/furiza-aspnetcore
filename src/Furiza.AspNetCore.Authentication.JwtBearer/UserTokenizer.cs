using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserTokenizer<TUserData> : IUserTokenizer<TUserData> where TUserData : IUserData
    {
        private readonly SecurityTokenHandler tokenHandler;
        private readonly JwtConfiguration jwtConfiguration;

        public UserTokenizer(SecurityTokenHandler tokenHandler,
            JwtConfiguration jwtConfiguration)
        {
            this.tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
            this.jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
        }

        public GenerateTokenResult GenerateToken(TUserData userData)
        {
            var identity = new ClaimsIdentity(
                new Claim[] 
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userData.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.GivenName, userData.FullName),
                    new Claim(JwtRegisteredClaimNames.Email, userData.Email),
                    new Claim(JwtRegisteredClaimNamesCustom.Company, userData.Company),
                    new Claim(JwtRegisteredClaimNamesCustom.Department, userData.Department)
                }
            );

            if (userData.Roles?.Any() ?? false)
                foreach (var role in userData.Roles)
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

            if (userData.Claims?.Any() ?? false)
                foreach (var claim in userData.Claims)
                    identity.AddClaim(new Claim(claim.Type, claim.Value));

            var securityToken = tokenHandler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = jwtConfiguration.Issuer,
                Audience = jwtConfiguration.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.Secret)), SecurityAlgorithms.HmacSha256),
                Subject = identity,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(jwtConfiguration.ExpirationInMinutes)
            });
            var jwt = tokenHandler.WriteToken(securityToken);

            return new GenerateTokenResult(jwt, GenerateRefreshToken(), securityToken.ValidTo);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber)
                    .Replace("+", string.Empty)
                    .Replace("=", string.Empty)
                    .Replace("/", string.Empty);
            }
        }
    }
}