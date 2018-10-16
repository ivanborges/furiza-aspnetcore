using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using Furiza.Base.Core.Exceptions.Serialization;
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
    public class UsersController : RootController
    {
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICacheHandler cacheHandler;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IUserNotifier emailSender;

        public UsersController(IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICacheHandler cacheHandler,
            IPasswordGenerator passwordGenerator,
            IUserNotifier emailSender)
        {
            this.mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            this.userManager = userManager ?? throw new System.ArgumentNullException(nameof(userManager));
            this.cacheHandler = cacheHandler ?? throw new System.ArgumentNullException(nameof(cacheHandler));
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
            if (filters.Role != null)
                p1 = u => u.IdentityUserRoles.Any(ur => ur.IdentityRole.Name == filters.Role.ToString());

            Func<ApplicationUser, bool> p2 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Company))
                p2 = u => u.Company == filters.Company;

            Func<ApplicationUser, bool> p3 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Department))
                p3 = u => u.Department == filters.Department;

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
            var user = await GetUserAsync(username);
            if (user == null)
                return NotFound();

            var result = mapper.Map<ApplicationUser, UsersGetResult>(user);

            return Ok(result);
        }

        [Authorize(Roles = "Superuser,Administrator")]
        [HttpPost]
        [ProducesResponseType(typeof(UsersPostResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync(UsersPost model)
        {
            if (await GetUserAsync(model.UserName) != null)
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