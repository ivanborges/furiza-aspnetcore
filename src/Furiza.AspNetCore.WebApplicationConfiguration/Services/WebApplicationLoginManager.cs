using Furiza.AspNetCore.Authentication.JwtBearer.Cookies;
using Furiza.AspNetCore.WebApplicationConfiguration.RestClients.Auth;
using Furiza.AspNetCore.WebApplicationConfiguration.RestClients.ReCaptcha;
using System;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.Services
{
    public class WebApplicationLoginManager
    {
        protected readonly ISecurityProviderClient securityProviderClient;
        protected readonly CookiesManager cookiesManager;
        protected readonly IReCaptchaClient reCaptchaClient;
        protected readonly reCaptchaConfiguration reCaptchaConfiguration;

        public WebApplicationLoginManager(ISecurityProviderClient securityProviderClient,
            CookiesManager cookiesManager,
            IReCaptchaClient reCaptchaClient,
            reCaptchaConfiguration reCaptchaConfiguration)
        {
            this.securityProviderClient = securityProviderClient ?? throw new ArgumentNullException(nameof(securityProviderClient));
            this.cookiesManager = cookiesManager ?? throw new ArgumentNullException(nameof(cookiesManager));
            this.reCaptchaClient = reCaptchaClient ?? throw new ArgumentNullException(nameof(reCaptchaClient));
            this.reCaptchaConfiguration = reCaptchaConfiguration ?? throw new ArgumentNullException(nameof(reCaptchaConfiguration));
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

        /// <summary>
        /// Verify a user's response to a reCAPTCHA challenge from the application's backend.
        /// </summary>
        /// <param name="userToken">The user response token provided by the reCAPTCHA client-side integration on the site (the value of 'g-recaptcha-response').</param>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task reCaptcha(string userToken)
        {
            var reCaptchaResponse = await reCaptchaClient.SiteVerifyAsync(reCaptchaConfiguration.SecretKey, userToken);
            if (!reCaptchaResponse.Success.HasValue || !reCaptchaResponse.Success.Value)
                throw new InvalidOperationException($"One or more errors occurred during reCaptcha bot verification with the following result: {string.Join(" | ", reCaptchaResponse.ErrorCodes)}");
        }
    }
}