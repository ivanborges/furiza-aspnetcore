using AutoMapper;
using Furiza.AspNetCore.ExceptionHandling;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.Identity.EntityFrameworkCore.Stores;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Roles;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions;
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

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Controllers.v1
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

            var result = new RolesGetManyResult()
            {
                Roles = mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RolesGetManyResult.RolesGetManyResultInnerRole>>(allRoles)
            };

            return Ok(result);
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> PostAsync([FromBody]RolesPost model)
        {
            if (await roleManager.FindByNameAsync(model.RoleName.Trim().ToLower()) != null)
                throw new RoleAlreadyExistsException();

            var role = mapper.Map<RolesPost, ApplicationRole>(model);
            var creationResult = await roleManager.CreateAsync(role);
            if (!creationResult.Succeeded)
                throw new IdentityOperationException(creationResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return NoContent();
        }

        [Authorize(Policy = FurizaPolicies.RequireAdministratorRights)]
        [HttpDelete("{roleName}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 404)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> DeleteAsync(string roleName,
            [FromServices]FurizaUserScopedRoleStore furizaUserScopedRoleStore)
        {
            foreach (FieldInfo fieldInfo in typeof(FurizaMasterRoles).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                if (fieldInfo.GetValue(typeof(FurizaMasterRoles)).ToString() == roleName.Trim().ToLower())
                    throw new DefaultRoleViolatedException();

            var role = await roleManager.Roles
                .Include(u => u.IdentityUserRoles)
                .SingleOrDefaultAsync(u => u.NormalizedName == roleName.Trim().ToUpper());

            if (role == null)
                throw new ResourceNotFoundException(new[] { SecurityResourceNotFoundExceptionItem.Role });

            if (role.IdentityUserRoles != null && role.IdentityUserRoles.Any())
                throw new RoleInUseException();

            if (await furizaUserScopedRoleStore.IsRoleInUse(roleName))
                throw new RoleInUseException();

            var deleteResult = await roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
                throw new IdentityOperationException(deleteResult.Errors.Select(e => new IdentityOperationExceptionItem(e.Code, e.Description)));

            return NoContent();
        }
    }
}