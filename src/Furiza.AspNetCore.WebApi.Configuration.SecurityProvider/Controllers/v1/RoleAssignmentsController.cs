using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.RoleAssignments;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services;
using Furiza.Base.Core.Exceptions.Serialization;
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
    [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
    public class RoleAssignmentsController : RootController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ICachedUserManager cachedUserManager;

        public RoleAssignmentsController(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ICachedUserManager cachedUserManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.cachedUserManager = cachedUserManager ?? throw new ArgumentNullException(nameof(cachedUserManager));
        }
        
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync([FromBody]RoleAssignmentsPost model)
        {
            var user = await GetApplicationUserAsync(model.UserName, model.RoleName);
            var operationResult = await userManager.AddToRoleAsync(user, model.RoleName);

            return await ParseIdentityResultAsync(model.UserName, operationResult);
        }

        [HttpDelete]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> DeleteAsync([FromBody]RoleAssignmentsDelete model)
        {
            var user = await GetApplicationUserAsync(model.UserName, model.RoleName);
            var operationResult = await userManager.RemoveFromRoleAsync(user, model.RoleName);

            return await ParseIdentityResultAsync(model.UserName, operationResult);            
        }

        #region [+] Pvts
        private async Task<ApplicationUser> GetApplicationUserAsync(string username, string rolename)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(username.Trim().ToLower());
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (!(await roleManager.RoleExistsAsync(rolename.Trim().ToLower())))
                errors.Add(SecurityResourceNotFoundExceptionItem.Role);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            return user;
        }

        private async Task<IActionResult> ParseIdentityResultAsync(string username, IdentityResult operationResult)
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