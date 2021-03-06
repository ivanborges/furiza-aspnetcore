﻿using AutoMapper;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    public class UsersController : RootController
    {
        private readonly string[] hiringTypes = new string[] { FurizaHiringTypes.InHouse, FurizaHiringTypes.Outsourced };

        private readonly IUserPrincipalBuilder userPrincipalBuilder;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICachedUserManager cachedUserManager;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IUserNotifier emailSender;

        public UsersController(IUserPrincipalBuilder userPrincipalBuilder,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICachedUserManager cachedUserManager,
            IPasswordGenerator passwordGenerator,
            IUserNotifier emailSender)
        {
            this.userPrincipalBuilder = userPrincipalBuilder ?? throw new ArgumentNullException(nameof(userPrincipalBuilder));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.cachedUserManager = cachedUserManager ?? throw new ArgumentNullException(nameof(cachedUserManager));
            this.passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        [HttpGet]
        [ProducesResponseType(typeof(UsersGetManyResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public IActionResult Get([FromQuery]UsersGetMany filters)
        {
            Func<ApplicationUser, bool> p1 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Role))
                p1 = u => u.IdentityUserRoles.Any(ur => ur.IdentityRole.Name.Trim().ToLower() == filters.Role.Trim().ToLower());

            Func<ApplicationUser, bool> p2 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.HiringType))
                p2 = u => u.HiringType?.Trim().ToLower() == filters.HiringType.Trim().ToLower();

            Func<ApplicationUser, bool> p3 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Company))
                p3 = u => u.Company?.Trim().ToLower() == filters.Company.Trim().ToLower();

            Func<ApplicationUser, bool> p4 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Department))
                p4 = u => u.Department?.Trim().ToLower() == filters.Department.Trim().ToLower();

            var allUsers = userManager.Users
                .Include(u => u.IdentityUserRoles)
                    .ThenInclude(ur => ur.IdentityRole)
                .Include(u => u.IdentityClaims)
                .Where(p1)
                .Where(p2)
                .Where(p3)
                .Where(p4)
                .OrderBy(u => u.UserName)
                .ToList();

            Parallel.ForEach(allUsers, user =>
            {
                user.IdentityUserRoles = user.IdentityUserRoles.Where(ur => ur.ClientId == userPrincipalBuilder.GetCurrentClientId()).ToList();
                user.IdentityClaims.Add(new ApplicationUserClaim()
                {
                    ClaimType = FurizaClaimNames.ClientId,
                    ClaimValue = userPrincipalBuilder.GetCurrentClientId().ToString()
                });
            });

            var resultItems = mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UsersGetResult>>(allUsers);
            var result = new UsersGetManyResult()
            {
                Users = resultItems
            };

            return Ok(result);
        }

        [HttpGet("{username}", Name = "UsersGet")]
        [ProducesResponseType(typeof(UsersGetResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> GetAsync([FromRoute]string username)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await cachedUserManager.GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(username, userPrincipalBuilder.GetCurrentClientId());
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var result = mapper.Map<ApplicationUser, UsersGetResult>(user);

            return Ok(result);
        }

        [HttpGet("byEmail")]
        [ProducesResponseType(typeof(UsersGetResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ByEmailGetAsync([FromQuery]UsersGetByEmail model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.Users
                .Include(u => u.IdentityUserRoles)
                    .ThenInclude(ur => ur.IdentityRole)
                .Include(u => u.IdentityClaims)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == model.Email.Trim().ToUpper());

            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            user.IdentityUserRoles = user.IdentityUserRoles.Where(ur => ur.ClientId == userPrincipalBuilder.GetCurrentClientId()).ToList();
            user.IdentityClaims.Add(new ApplicationUserClaim()
            {
                ClaimType = FurizaClaimNames.ClientId,
                ClaimValue = userPrincipalBuilder.GetCurrentClientId().ToString()
            });

            var result = mapper.Map<ApplicationUser, UsersGetResult>(user);

            return Ok(result);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost]
        [ProducesResponseType(typeof(PostResult), 201)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync([FromBody]UsersPost model)
        {
            if (await cachedUserManager.GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(model.UserName, userPrincipalBuilder.GetCurrentClientId()) != null)
                throw new UserAlreadyExistsException();

            if (await userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == model.Email.Trim().ToUpper()) != null)
                throw new EmailAlreadyExistsException();

            if (!hiringTypes.Contains(model.HiringType))
                throw new InvalidHiringTypeException();

            var user = mapper.Map<UsersPost, ApplicationUser>(model);
            user.EmailConfirmed = !model.GeneratePassword && string.IsNullOrWhiteSpace(model.Password);

            var password = model.GeneratePassword
                ? passwordGenerator.GenerateRandomPassword()
                : model.Password;

            var creationResult = !string.IsNullOrWhiteSpace(password)
                ? await userManager.CreateAsync(user, password)
                : await userManager.CreateAsync(user);

            if (creationResult.Succeeded)
            {
                if (!user.EmailConfirmed)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.RouteUrl("ConfirmEmail", new { username = user.UserName, token }, Request.Scheme);

                    await emailSender.NotifyNewUserAsync(model.Email, model.UserName, callbackUrl, model.GeneratePassword ? password : null);
                }

                return CreatedAtRoute("UsersGet", new { username = user.UserName }, new PostResult(user.Id, user.UserName));
            }
            else
                throw new IdentityOperationException(creationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));
        }

        [HttpPost("ChangePassword")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ChangePasswordPostAsync([FromBody]ChangePasswordUsersPost model)
        {
            var user = await userManager.FindByNameAsync(userPrincipalBuilder.UserPrincipal.UserName);
            var operationResult = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            return await ParseIdentityResultAndReturnAsync(userPrincipalBuilder.UserPrincipal.UserName, operationResult);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost("{username}/ResetPassword")]
        [ProducesResponseType(typeof(ResetPasswordUsersPostResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ResetPasswordPostAsync(string username)
        {
            var user = await GetApplicationUserAsync(username);
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = passwordGenerator.GenerateRandomPassword();
            var operationResult = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (operationResult.Succeeded)
            {
                await cachedUserManager.RemoveUserByUserNameAsync(username);
                await emailSender.NotifyUserPasswordResetAsync(user.Email, user.UserName, newPassword);

                return Ok(new ResetPasswordUsersPostResult() { NewPassword = newPassword });
            }
            else
                throw new IdentityOperationException(operationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost("{username}/ConfirmEmail")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ConfirmEmailPostAsync(string username)
        {
            var user = await GetApplicationUserAsync(username);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var operationResult = await userManager.ConfirmEmailAsync(user, token);

            return await ParseIdentityResultAndReturnAsync(username, operationResult);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost("{username}/Lock")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> LockPostAsync(string username)
        {
            var user = await GetApplicationUserAsync(username);
            var operationResult = await userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddYears(100));

            return await ParseIdentityResultAndReturnAsync(username, operationResult);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost("{username}/Unlock")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> UnlockPostAsync(string username)
        {
            var user = await GetApplicationUserAsync(username);
            var operationResult = await userManager.SetLockoutEndDateAsync(user, null);

            return await ParseIdentityResultAndReturnAsync(username, operationResult);
        }        

        [AllowAnonymous]
        [HttpGet("ConfirmEmail", Name = "ConfirmEmail")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ConfirmEmailGetAsync([FromQuery]ConfirmEmailGet model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var confirmationResult = await userManager.ConfirmEmailAsync(user, model.Token);
            if (!confirmationResult.Succeeded)
                throw new IdentityOperationException(confirmationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return NoContent();
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost("{username}/Claims")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ModifyClaimPostAsync(string username, [FromBody]ModifyClaimPost model)
        {
            var user = await GetApplicationUserAsync(username);
            var claim = new Claim(model.ClaimType, model.ClaimValue);

            var operationResult = model.Operation == ModifyClaimOperation.Add
                ? await userManager.AddClaimAsync(user, claim)
                : await userManager.RemoveClaimAsync(user, claim);

            return await ParseIdentityResultAndReturnAsync(username, operationResult);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpDelete("{username}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> DeleteAsync(string username)
        {
            var user = await GetApplicationUserAsync(username);
            var operationResult = await userManager.DeleteAsync(user);

            return await ParseIdentityResultAndReturnAsync(username, operationResult);
        }

        #region [+] Pvts
        private async Task<ApplicationUser> GetApplicationUserAsync(string username)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(username.Trim().ToLower());
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            return user;
        }

        private async Task<IActionResult> ParseIdentityResultAndReturnAsync(string username, IdentityResult operationResult)
        {
            if (operationResult.Succeeded)
            {
                await cachedUserManager.RemoveUserByUserNameAsync(username);

                return NoContent();
            }
            else
                throw new IdentityOperationException(operationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));
        }
        #endregion
    }
}