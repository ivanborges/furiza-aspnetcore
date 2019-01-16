using Furiza.AspNetCore.Authentication.JwtBearer.Cookies;
using Furiza.AspNetCore.WebApplicationConfiguration.RestClients;
using System;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.Services
{
    public class WebApplicationLoginManager
    {
        protected readonly ISecurityProviderClient securityProviderClient;
        protected readonly CookiesManager cookiesManager;

        public WebApplicationLoginManager(ISecurityProviderClient securityProviderClient,
            CookiesManager cookiesManager)
        {
            this.securityProviderClient = securityProviderClient ?? throw new ArgumentNullException(nameof(securityProviderClient));
            this.cookiesManager = cookiesManager ?? throw new ArgumentNullException(nameof(cookiesManager));
        }

        public virtual async Task LoginAsync(Guid clientId, string user, string password)
        {
            var authPost = new AuthPost()
            {
                GrantType = GrantType.Password,
                ClientId = clientId,
                User = user,
                Password = password
            };

            var authPostResult = await securityProviderClient.AuthAsync(authPost);

            await cookiesManager.CreateCookieAsync(authPostResult.AccessToken, authPostResult.RefreshToken);
        }

        public virtual async Task LogoutAsync()
        {
            await cookiesManager.DismissCookieAsync();
        }
    }
}