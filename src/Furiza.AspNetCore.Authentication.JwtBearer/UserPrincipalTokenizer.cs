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
    internal class UserPrincipalTokenizer : IUserPrincipalTokenizer
    {
        private readonly SecurityTokenHandler tokenHandler;
        private readonly JwtConfiguration jwtConfiguration;

        public UserPrincipalTokenizer(SecurityTokenHandler tokenHandler,
            JwtConfiguration jwtConfiguration)
        {
            this.tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
            this.jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
        }

        public GenerateTokenResult GenerateToken<TUserPrincipal>(TUserPrincipal userPrincipal)
            where TUserPrincipal : IUserPrincipal
        {
            var clientIdClaim = userPrincipal.Claims?.SingleOrDefault(c => c.Type == FurizaClaimNames.ClientId) ?? 
                throw new InvalidOperationException("User principal does not contain a valid claim for ClientId.");

            var identity = new ClaimsIdentity(
                new Claim[] 
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.Sub, userPrincipal.UserName)
                }
            );

            if (!string.IsNullOrWhiteSpace(userPrincipal.FullName))
                identity.AddClaim(new Claim(JwtRegisteredClaimNames.GivenName, userPrincipal.FullName));

            if (!string.IsNullOrWhiteSpace(userPrincipal.Email))
                identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, userPrincipal.Email));

            if (!string.IsNullOrWhiteSpace(userPrincipal.HiringType))
                identity.AddClaim(new Claim(FurizaClaimNames.HiringType, userPrincipal.HiringType));

            if (!string.IsNullOrWhiteSpace(userPrincipal.Company))
                identity.AddClaim(new Claim(FurizaClaimNames.Company, userPrincipal.Company));

            if (!string.IsNullOrWhiteSpace(userPrincipal.Department))
                identity.AddClaim(new Claim(FurizaClaimNames.Department, userPrincipal.Department));

            if (userPrincipal.Claims != null && userPrincipal.Claims.Any())
                foreach (var claim in userPrincipal.Claims.Where(c => c.Type != FurizaClaimNames.ClientId))
                    identity.AddClaim(new Claim(claim.Type, claim.Value));

            if (userPrincipal.RoleAssignments != null && userPrincipal.RoleAssignments.Any())
                foreach (var ra in userPrincipal.RoleAssignments)
                    identity.AddClaim(new Claim(ClaimTypes.Role, ra.Role));

            var securityToken = tokenHandler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = jwtConfiguration.Issuer,
                Audience = clientIdClaim.Value,
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