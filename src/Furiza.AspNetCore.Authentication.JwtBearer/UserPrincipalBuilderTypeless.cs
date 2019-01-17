using Furiza.AspNetCore.Authentication.JwtBearer.Identity;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserPrincipalBuilderTypeless : UserPrincipalBuilderBase<GenericUserPrincipal, GenericScopedRoleAssignment>, IUserPrincipalBuilder
    {
        public override GenericUserPrincipal UserPrincipal => ValidateClaimsAndBuildUserPrincipal();

        public UserPrincipalBuilderTypeless(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
            : base(httpContextAccessor, scopedRoleAssignmentProvider)
        {
        }

        IUserPrincipal IUserPrincipalBuilder<IUserPrincipal, IScopedRoleAssignment>.UserPrincipal => UserPrincipal;
        async Task<IEnumerable<IScopedRoleAssignment>> IUserPrincipalBuilder<IUserPrincipal, IScopedRoleAssignment>.GetScopedRoleAssignmentsAsync() => await base.GetScopedRoleAssignmentsAsync();
    }
}