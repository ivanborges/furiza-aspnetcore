﻿using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
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
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    public class UsersController : RootController
    {
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
            if (!string.IsNullOrWhiteSpace(filters.Company))
                p2 = u => u.Company.Trim().ToLower() == filters.Company.Trim().ToLower();

            Func<ApplicationUser, bool> p3 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Department))
                p3 = u => u.Department.Trim().ToLower() == filters.Department.Trim().ToLower();

            var allUsers = userManager.Users
                .Include(u => u.IdentityUserRoles)
                    .ThenInclude(ur => ur.IdentityRole)
                .Include(u => u.IdentityClaims)
                .Where(p1)
                .Where(p2)
                .Where(p3)
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
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> GetAsync(string username)
        {
            var clientId = new Guid(userPrincipalBuilder.UserPrincipal.Claims.Single(c => c.Type == FurizaClaimNames.ClientId).Value);
            var user = await cachedUserManager.GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(username, clientId);
            if (user == null)
                return NotFound();

            var result = mapper.Map<ApplicationUser, UsersGetResult>(user);

            return Ok(result);
        }

        [Authorize(Roles = FurizaMasterRoles.Superuser + "," + FurizaMasterRoles.Administrator)] // TODO: criar policy...
        [HttpPost]
        [ProducesResponseType(typeof(UsersPostResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync(UsersPost model)
        {
            var clientId = new Guid(userPrincipalBuilder.UserPrincipal.Claims.Single(c => c.Type == FurizaClaimNames.ClientId).Value);
            if (await cachedUserManager.GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(model.UserName, clientId) != null)
                throw new UserAlreadyExistsException();

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
            {
                var errors = new List<IdentityOperationExceptionItem>();
                foreach (var error in creationResult.Errors)
                    errors.Add(new IdentityOperationExceptionItem(error.Code, error.Description));

                throw new IdentityOperationException(errors);
            }

            return Ok(new UsersPostResult() { Succeeded = true });
        }

        [AllowAnonymous]
        [HttpGet("ConfirmEmail", Name = "ConfirmEmail")]
        [ProducesResponseType(typeof(ConfirmEmailGetResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery]ConfirmEmailGet values)
        {
            var user = await userManager.FindByNameAsync(values.UserName);
            if (user == null)
                return NotFound();

            var confirmationResult = await userManager.ConfirmEmailAsync(user, values.Token);
            if (!confirmationResult.Succeeded)
            {
                var errors = new List<IdentityOperationExceptionItem>();
                foreach (var error in confirmationResult.Errors)
                    errors.Add(new IdentityOperationExceptionItem(error.Code, error.Description));

                throw new IdentityOperationException(errors);
            }

            // TODO: substituir retornos Json por Views... já que o usuário final acessa esse endereço diretamente pelo browser.
            return Ok(new ConfirmEmailGetResult() { Succeeded = true });
        }

        // TODO: criar métodos de adicionar e remover roles e claims.
        // TODO: customizar o Identity para os métodos AddClaimAsync e AddToRoleAsync do UserManager gravarem o creationdate e creationuser.
    }
}