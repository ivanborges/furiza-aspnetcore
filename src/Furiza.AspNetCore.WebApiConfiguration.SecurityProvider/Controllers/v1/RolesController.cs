using AutoMapper;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.Identity.EntityFrameworkCore.Stores;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Roles;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    public class RolesController : RootController
    {
        private readonly IMapper mapper;
        private readonly RoleManager<ApplicationRole> roleManager;

        public RolesController(IMapper mapper,
            RoleManager<ApplicationRole> roleManager)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        [HttpGet]
        [ProducesResponseType(typeof(RolesGetManyResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> GetAsync()
        {
            var allRoles = await roleManager.Roles
                .OrderBy(u => u.Name)
                .ToListAsync();

            var resultItems = mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RolesGetResult>>(allRoles);
            var result = new RolesGetManyResult()
            {
                Roles = resultItems
            };

            return Ok(result);
        }

        [HttpGet("{rolename}")]
        [ProducesResponseType(typeof(RolesGetResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> GetAsync(string rolename)
        {
            var errors = new List<SecurityResourceNotFoundExceptionItem>();

            var role = await roleManager.FindByNameAsync(rolename.Trim().ToLower());
            if (role == null)
                errors.Add(SecurityResourceNotFoundExceptionItem.Role);

            if (errors.Any())
                throw new ResourceNotFoundException(errors);

            var result = mapper.Map<ApplicationRole, RolesGetResult>(role);

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
        public async Task<IActionResult> PostAsync([FromBody]RolesPost model)
        {
            if (await roleManager.FindByNameAsync(model.RoleName.Trim().ToLower()) != null)
                throw new UserAlreadyExistsException();

            var role = mapper.Map<RolesPost, ApplicationRole>(model);
            var creationResult = await roleManager.CreateAsync(role);
            if (!creationResult.Succeeded)
                throw new IdentityOperationException(creationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpDelete("{rolename}")]
        [ProducesResponseType(typeof(IdentityOperationResult), 200)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> DeleteAsync(string rolename,
            [FromServices]FurizaUserScopedRoleStore furizaUserScopedRoleStore)
        {
            foreach (FieldInfo fieldInfo in typeof(FurizaMasterRoles).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                if (fieldInfo.GetValue(typeof(FurizaMasterRoles)).ToString() == rolename.Trim().ToLower())
                    throw new DefaultRoleViolatedException();

            var role = await roleManager.Roles
                .Include(u => u.IdentityUserRoles)
                .SingleOrDefaultAsync(u => u.NormalizedName == rolename.Trim().ToUpper());

            if (role == null)
                throw new ResourceNotFoundException(new[] { SecurityResourceNotFoundExceptionItem.Role });

            if (role.IdentityUserRoles != null && role.IdentityUserRoles.Any())
                throw new RoleInUseException();

            if (await furizaUserScopedRoleStore.IsRoleInUse(rolename))
                throw new RoleInUseException();

            var deleteResult = await roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
                throw new IdentityOperationException(deleteResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return Ok(new IdentityOperationResult() { Succeeded = true });
        }
    }
}