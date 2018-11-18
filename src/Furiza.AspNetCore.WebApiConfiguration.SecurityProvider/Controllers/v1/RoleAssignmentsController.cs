using AutoMapper;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.RoleAssignments;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
    public class RoleAssignmentsController : RootController
    {
        private readonly IUserPrincipalBuilder userPrincipalBuilder;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ICachedUserManager cachedUserManager;

        public RoleAssignmentsController(IUserPrincipalBuilder userPrincipalBuilder,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ICachedUserManager cachedUserManager)
        {
            this.userPrincipalBuilder = userPrincipalBuilder ?? throw new ArgumentNullException(nameof(userPrincipalBuilder));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.cachedUserManager = cachedUserManager ?? throw new ArgumentNullException(nameof(cachedUserManager));
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync([FromBody]RoleAssignmentsPost model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (!(await roleManager.RoleExistsAsync(model.Role)))
                errors.Add(SecurityResourceNotFoundExceptionItem.Role);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var creationResult = await userManager.AddToRoleAsync(user, model.Role);
            if (creationResult.Succeeded)
                await cachedUserManager.RemoveUserByUserNameAsync(model.UserName);
            else
                throw new IdentityOperationException(creationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [HttpDelete]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> DeleteAsync([FromBody]RoleAssignmentsDelete model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (!(await roleManager.RoleExistsAsync(model.Role)))
                errors.Add(SecurityResourceNotFoundExceptionItem.Role);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var removeResult = await userManager.RemoveFromRoleAsync(user, model.Role);
            if (removeResult.Succeeded)
                await cachedUserManager.RemoveUserByUserNameAsync(model.UserName);
            else
                throw new IdentityOperationException(removeResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }
    }
}