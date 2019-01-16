using AutoMapper;
using Furiza.AspNetCore.Authentication.JwtBearer.Cookies.Services;
using Furiza.AspNetCore.WebApplicationConfiguration.RestClients;
using System;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.Services
{
    internal class AccessTokenRefresher : IAccessTokenRefresher
    {
        private readonly ISecurityProviderClient securityProviderClient;

        public AccessTokenRefresher(ISecurityProviderClient securityProviderClient)
        {
            this.securityProviderClient = securityProviderClient ?? throw new ArgumentNullException(nameof(securityProviderClient));
        }

        public async Task<RefreshTokenResult> RefreshAsync(Guid clientId, string refreshToken)
        {
            var authPost = new AuthPost()
            {
                GrantType = GrantType.RefreshToken,
                ClientId = clientId,
                RefreshToken = refreshToken
            };

            var authPostResult = await securityProviderClient.AuthAsync(authPost);
            var refreshTokenResult = Mapper.Map<AuthPostResult, RefreshTokenResult>(authPostResult);

            return refreshTokenResult;
        }
    }
}