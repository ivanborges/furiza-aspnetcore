using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
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

        public UsersController(IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICacheHandler cacheHandler)
        {
            this.mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            this.userManager = userManager ?? throw new System.ArgumentNullException(nameof(userManager));
            this.cacheHandler = cacheHandler ?? throw new System.ArgumentNullException(nameof(cacheHandler));
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetManyResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public IActionResult Get([FromQuery]GetMany filters)
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

            var resultItems = mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<GetResult>>(allUsers);
            var result = new GetManyResult()
            {
                Users = resultItems
            };

            return Ok(result);
        }

        [HttpGet("{username}")]
        [ProducesResponseType(typeof(GetResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> GetAsync(string username)
        {
            var user = await GetUserAsync(username);
            if (user == null)
                return NotFound();

            var result = mapper.Map<ApplicationUser, GetResult>(user);

            return Ok(result);
        }

        [Authorize(Roles = "Superuser,Administrator")]
        [HttpPost]
        [ProducesResponseType(typeof(GetResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync(Post userPost)
        {
            if (await GetUserAsync(userPost.UserName) != null)
                throw new UserAlreadyExistsException();

            var user = mapper.Map<Post, ApplicationUser>(userPost);
            user.EmailConfirmed = !userPost.GeneratePassword.Value;

            var creationResult = userPost.GeneratePassword.Value
                ? await userManager.CreateAsync(user, "password")
                : await userManager.CreateAsync(user);

            if (!creationResult.Succeeded)
            {
                var errors = new List<IdentityOperationExceptionItem>();
                foreach (var error in creationResult.Errors)
                    errors.Add(new IdentityOperationExceptionItem(error.Code, error.Description));

                throw new IdentityOperationException(errors);
            }

            return Ok(new PostResult() { Succeeded = true });
        }

        private async Task<ApplicationUser> GetUserAsync(string username)
        {
            var normalizedUserName = username.ToUpper().Trim();
            if (!cacheHandler.TryGetValue<ApplicationUser>(normalizedUserName, out var userIdentity))
            {
                userIdentity = await userManager.Users
                    .Include(u => u.IdentityUserRoles)
                        .ThenInclude(ur => ur.IdentityRole)
                    .Include(u => u.IdentityClaims)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);

                if (userIdentity != null)
                    await cacheHandler.SetAsync(normalizedUserName, userIdentity);
            }

            return userIdentity;
        }
    }
}