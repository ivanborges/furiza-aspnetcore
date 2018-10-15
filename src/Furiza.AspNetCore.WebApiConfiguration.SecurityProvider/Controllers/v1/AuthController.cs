using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Auth;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Furiza.Caching.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AuthController : RootController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ISignInManager<ApplicationUser> signInManager;
        private readonly IUserTokenizer<ApplicationUser> userTokenizer;
        private readonly ICacheHandler cacheHandler;

        public AuthController(UserManager<ApplicationUser> userManager,
            ISignInManager<ApplicationUser> signInManager,
            IUserTokenizer<ApplicationUser> userTokenizer,
            ICacheHandler cacheHandler)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userTokenizer = userTokenizer ?? throw new ArgumentNullException(nameof(userTokenizer));
            this.cacheHandler = cacheHandler ?? throw new ArgumentNullException(nameof(cacheHandler));
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(AuthPostResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync([FromBody]AuthPost model)
        {
            var result = Unauthorized() as IActionResult;
            var errors = new List<AuthExceptionItem>();
            var user = null as ApplicationUser;
            var isAuthenticated = false;

            switch (model.GrantType)
            {
                case GrantType.Password:
                    if (string.IsNullOrWhiteSpace(model.User))
                        errors.Add(AuthExceptionItem.UserRequired);

                    if (string.IsNullOrWhiteSpace(model.Password))
                        errors.Add(AuthExceptionItem.PasswordRequired);

                    if (errors.Any())
                        throw new AuthException(errors);

                    user = await GetUserAsync(model.User);
                    if (user != null)
                    {
                        if (!await userManager.IsEmailConfirmedAsync(user))
                            throw new EmailConfirmationRequiredException();

                        var checkPassword = await signInManager.CheckPasswordSignInAsync(user, model.Password);
                        if (checkPassword)
                            isAuthenticated = true;
                    }

                    break;
                case GrantType.RefreshToken:
                    if (string.IsNullOrWhiteSpace(model.RefreshToken))
                        errors.Add(AuthExceptionItem.RefreshTokenRequired);

                    if (errors.Any())
                        throw new AuthException(errors);

                    if (cacheHandler.TryGetValue<RefreshTokenData>(model.RefreshToken, out var refreshTokenData))
                    {
                        await cacheHandler.RemoveAsync<RefreshTokenData>(model.RefreshToken);
                        user = await GetUserAsync(refreshTokenData.UserName);
                        isAuthenticated = true;
                    }

                    break;
            }

            if (isAuthenticated)
            {
                var authPostResult = new AuthPostResult(userTokenizer.GenerateToken(user));
                await cacheHandler.SetAsync(authPostResult.RefreshToken, new RefreshTokenData()
                {
                    Token = authPostResult.RefreshToken,
                    UserName = user.UserName
                });
                result = Ok(authPostResult);
            }

            return result;
        }

        private async Task<ApplicationUser> GetUserAsync(string username)
        {
            var normalizedUserName = username.ToUpper().Trim();
            if (!cacheHandler.TryGetValue<ApplicationUser>(normalizedUserName, out var user))
            {
                user = await userManager.Users
                    .Include(u => u.IdentityUserRoles)
                        .ThenInclude(ur => ur.IdentityRole)
                    .Include(u => u.IdentityClaims)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);

                if (user != null && user.EmailConfirmed && user.Roles.Any())
                    await cacheHandler.SetAsync(normalizedUserName, user);
            }

            return user;
        }
    }
}