using AutoMapper;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.Identity.EntityFrameworkCore.Stores;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.ScopedRoleAssignments;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
using Furiza.Base.Core.Exceptions.Serialization;
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
    public class ScopedRoleAssignmentsController : RootController
    {
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly FurizaUserScopedRoleStore furizaUserScopedRoleStore;
        private readonly IdentityErrorDescriber identityErrorDescriber;

        public ScopedRoleAssignmentsController(IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            FurizaUserScopedRoleStore furizaUserScopedRoleStore,
            IdentityErrorDescriber identityErrorDescriber = null)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.furizaUserScopedRoleStore = furizaUserScopedRoleStore ?? throw new ArgumentNullException(nameof(furizaUserScopedRoleStore));
            this.identityErrorDescriber = identityErrorDescriber ?? throw new ArgumentNullException(nameof(identityErrorDescriber));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ScopedRoleAssignmentsGetManyResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public IActionResult Get([FromQuery]ScopedRoleAssignmentsGetMany filters)
        {
            Func<ApplicationUserScopedRole, bool> p1 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.UserName))
                p1 = usr => usr.UserName == filters.UserName.Trim().ToLower();

            Func<ApplicationUserScopedRole, bool> p2 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Role))
                p2 = usr => usr.Role == filters.Role.Trim().ToLower();

            Func<ApplicationUserScopedRole, bool> p3 = u => true;
            if (!string.IsNullOrWhiteSpace(filters.Scope))
                p3 = usr => usr.Scope == filters.Scope.Trim().ToLower();

            var allScopedRoleAssignments = furizaUserScopedRoleStore.ScopedRoleAssignments
                .Where(p1)
                .Where(p2)
                .Where(p3)
                .OrderBy(usr => usr.UserName)
                    .ThenBy(usr => usr.Scope)
                        .ThenBy(usr => usr.Role)
                .ToList();

            // TODO: adicionar cache 

            var resultItems = mapper.Map<IEnumerable<ApplicationUserScopedRole>, IEnumerable<ScopedRoleAssignmentsGetResult>>(allScopedRoleAssignments);
            var result = new ScopedRoleAssignmentsGetManyResult()
            {
                ScopedRoleAssignments = resultItems
            };

            return Ok(result);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync([FromBody]ScopedRoleAssignmentsPost model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (!(await roleManager.RoleExistsAsync(model.Role)))
                errors.Add(SecurityResourceNotFoundExceptionItem.Role);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            if (await furizaUserScopedRoleStore.IsInScopedRoleAsync(model.UserName, model.Role, model.Scope))
            {
                var errorDescription = identityErrorDescriber.UserAlreadyInRole($"{model.Role}.{model.Scope}");
                throw new IdentityOperationException(new IdentityOperationExceptionItem[] { new IdentityOperationExceptionItem(errorDescription.Code, errorDescription.Description) });
            }

            await furizaUserScopedRoleStore.AddToScopedRoleAsync(model.UserName, model.Role, model.Scope);

            // TODO: atualizar cache

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpDelete]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> DeleteAsync([FromBody]ScopedRoleAssignmentsDelete model)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.User);

            if (!(await roleManager.RoleExistsAsync(model.Role)))
                errors.Add(SecurityResourceNotFoundExceptionItem.Role);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            if (!(await furizaUserScopedRoleStore.IsInScopedRoleAsync(model.UserName, model.Role, model.Scope)))
            {
                var errorDescription = identityErrorDescriber.UserNotInRole($"{model.Role}.{model.Scope}");
                throw new IdentityOperationException(new IdentityOperationExceptionItem[] { new IdentityOperationExceptionItem(errorDescription.Code, errorDescription.Description) });
            }

            await furizaUserScopedRoleStore.RemoveFromScopedRoleAsync(model.UserName, model.Role, model.Scope);

            // TODO: atualizar cache

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }
    }
}