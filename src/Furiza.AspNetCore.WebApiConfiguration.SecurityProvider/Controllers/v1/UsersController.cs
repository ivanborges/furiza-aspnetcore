using AutoMapper;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
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

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Controllers.v1
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

            var resultItems = mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UsersGetResult>>(allUsers);
            var result = new UsersGetManyResult()
            {
                Users = resultItems
            };

            return Ok(result);
        }

        [HttpGet("{username}")]
        [ProducesResponseType(typeof(UsersGetResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> GetAsync(string username)
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

        [HttpGet("byemail")]
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

            var result = mapper.Map<ApplicationUser, UsersGetResult>(user);

            return Ok(result);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
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
            }
            else
                throw new IdentityOperationException(creationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ChangePasswordPostAsync([FromBody]ChangePasswordUsersPost model)
        {
            var user = await userManager.FindByNameAsync(userPrincipalBuilder.UserPrincipal.UserName);
            var operationResult = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (operationResult.Succeeded)
                await cachedUserManager.RemoveUserByUserNameAsync(userPrincipalBuilder.UserPrincipal.UserName);
            else
                throw new IdentityOperationException(operationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost("{username}/ResetPassword")]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ResetPasswordPostAsync(string username)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(username.Trim().ToLower());
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = passwordGenerator.GenerateRandomPassword();
            var operationResult = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (operationResult.Succeeded)
            {
                await cachedUserManager.RemoveUserByUserNameAsync(username);
                await emailSender.NotifyUserPasswordResetAsync(user.Email, user.UserName, newPassword);
            }
            else
                throw new IdentityOperationException(operationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [AllowAnonymous]
        [HttpGet("ConfirmEmail", Name = "ConfirmEmail")]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
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

            // TODO: substituir retornos Json por Views... já que o usuário final acessa esse endereço diretamente pelo browser.
            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPatch("{username}")]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ModifyClaimPatchAsync(string username, [FromBody]ModifyClaimPatch model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(username.Trim().ToLower());
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var claim = new Claim(model.ClaimType, model.ClaimValue);

            var operationResult = model.Operation == ModifyClaimOperation.Add
                ? await userManager.AddClaimAsync(user, claim)
                : await userManager.RemoveClaimAsync(user, claim);

            if (operationResult.Succeeded)
                await cachedUserManager.RemoveUserByUserNameAsync(username);
            else
                throw new IdentityOperationException(operationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }
    }
}