using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserPrincipalBuilderTyped<TUserPrincipal, TScopedRoleAssignment> : UserPrincipalBuilderBase<TUserPrincipal, TScopedRoleAssignment>
        where TUserPrincipal : IUserPrincipal, new()
        where TScopedRoleAssignment : IScopedRoleAssignment
    {
        public override TUserPrincipal UserPrincipal => ValidateClaimsAndBuildUserPrincipal();

        public UserPrincipalBuilderTyped(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
            : base(httpContextAccessor, scopedRoleAssignmentProvider)
        {
        }
    }
}