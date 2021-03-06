﻿using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Auth;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Furiza.Caching.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AuthController : RootController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICachedUserManager cachedUserManager;
        private readonly ISignInManager signInManager;
        private readonly IUserPrincipalTokenizer userPrincipalTokenizer;
        private readonly ICacheHandler cacheHandler;

        public AuthController(UserManager<ApplicationUser> userManager,
            ICachedUserManager cachedUserManager,
            ISignInManager signInManager,
            IUserPrincipalTokenizer userPrincipalTokenizer,
            ICacheHandler cacheHandler)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.cachedUserManager = cachedUserManager ?? throw new ArgumentNullException(nameof(cachedUserManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userPrincipalTokenizer = userPrincipalTokenizer ?? throw new ArgumentNullException(nameof(userPrincipalTokenizer));
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
            
            var user = null as ApplicationUser;
            var isAuthenticated = false;

            switch (model.GrantType)
            {
                case GrantType.Password:
                    ValidateModelForGrantTypePassword(model);

                    user = await cachedUserManager.GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(model.User, model.ClientId.Value);
                    if (user != null)
                    {
                        await ValidateUserStateAsync(user);

                        var checkPassword = await signInManager.CheckPasswordSignInAsync(user, model.Password);
                        if (checkPassword)
                            isAuthenticated = true;
                    }

                    break;
                case GrantType.RefreshToken:
                    ValidateModelForGrantTypeRefreshToken(model);

                    if (cacheHandler.TryGetValue<RefreshTokenData>(model.RefreshToken, out var refreshTokenData))
                    {
                        await cacheHandler.RemoveAsync<RefreshTokenData>(model.RefreshToken);
                        user = await cachedUserManager.GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(refreshTokenData.UserName, model.ClientId.Value);
                        isAuthenticated = true;
                    }

                    break;
            }

            if (isAuthenticated)
            {
                var authPostResult = new AuthPostResult(userPrincipalTokenizer.GenerateToken(user));
                await cacheHandler.SetAsync(authPostResult.RefreshToken, new RefreshTokenData()
                {
                    Token = authPostResult.RefreshToken,
                    UserName = user.UserName
                });
                result = Ok(authPostResult);
            }

            return result;
        }

        #region [+] Privates
        private void ValidateModelForGrantTypeRefreshToken(AuthPost model)
        {
            var errors = new List<AuthExceptionItem>();

            if (string.IsNullOrWhiteSpace(model.RefreshToken))
                errors.Add(AuthExceptionItem.RefreshTokenRequired);

            if (errors.Any())
                throw new AuthException(errors);
        }

        private void ValidateModelForGrantTypePassword(AuthPost model)
        {
            var errors = new List<AuthExceptionItem>();

            if (string.IsNullOrWhiteSpace(model.User))
                errors.Add(AuthExceptionItem.UserRequired);

            if (string.IsNullOrWhiteSpace(model.Password))
                errors.Add(AuthExceptionItem.PasswordRequired);

            if (errors.Any())
                throw new AuthException(errors);
        }

        private async Task ValidateUserStateAsync(ApplicationUser user)
        {
            var errors = new List<AuthExceptionItem>();

            if (await userManager.IsLockedOutAsync(user))
                errors.Add(AuthExceptionItem.UserLockedOut);

            if (!await userManager.IsEmailConfirmedAsync(user))
                errors.Add(AuthExceptionItem.EmailConfirmationRequired);

            if (!user.RoleAssignments.Any())
                errors.Add(AuthExceptionItem.RoleAssignmentRequired);

            if (errors.Any())
                throw new AuthException(errors);
        }
        #endregion
    }
}