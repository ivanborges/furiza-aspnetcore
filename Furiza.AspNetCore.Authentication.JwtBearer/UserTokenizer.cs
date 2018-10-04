using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserTokenizer<TUserData> : IUserTokenizer<TUserData> where TUserData : IUserData
    {
        private readonly SecurityTokenHandler tokenHandler;
        private readonly JwtConfiguration jwtConfiguration;
        private readonly SigningConfiguration signingConfiguration;

        public UserTokenizer(SecurityTokenHandler tokenHandler,
            JwtConfiguration jwtConfiguration,
            SigningConfiguration signingConfiguration)
        {
            this.tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
            this.jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
            this.signingConfiguration = signingConfiguration ?? throw new ArgumentNullException(nameof(signingConfiguration));
        }

        public GenerateTokenResult GenerateToken(TUserData userData)
        {
            var identity = new ClaimsIdentity(
                new GenericIdentity(userData.UserName, nameof(userData.UserName)), // TODO: testar sem generic identity
                new Claim[] 
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.UniqueName, userData.UserName),
                    new Claim(JwtRegisteredClaimNames.GivenName, userData.FullName),
                    new Claim(JwtRegisteredClaimNames.Email, userData.Email),
                    new Claim(JwtRegisteredClaimNamesCustom.Company, userData.Company),
                    new Claim(JwtRegisteredClaimNamesCustom.Department, userData.Department)
                }
            );

            if (userData.Roles.Any())
                foreach (var role in userData.Roles)
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

            if (userData.Claims.Any())
                foreach (var claim in userData.Claims)
                    identity.AddClaim(new Claim(claim.Type, claim.Value));

            //var expirationDate = DateTime.UtcNow.AddMinutes(jwtConfiguration.ExpirationInMinutes); // TODO: verificar no struct se deu certo 

            var securityToken = tokenHandler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = jwtConfiguration.Issuer,
                Audience = jwtConfiguration.Audience,
                SigningCredentials = signingConfiguration.SigningCredentials,
                Subject = identity,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(jwtConfiguration.ExpirationInMinutes)
            });
            var jwt = tokenHandler.WriteToken(securityToken); // TODO: verificar e tokenHandler é do tipo JwtSecurityTokenHandler

            return new GenerateTokenResult(jwt, string.Empty, securityToken.ValidTo);
        }
    }
}