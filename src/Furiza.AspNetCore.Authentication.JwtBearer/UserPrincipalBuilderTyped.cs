using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserPrincipalBuilderTyped<TUserPrincipal, TScopedRoleAssignment> : UserPrincipalBuilderBase<TUserPrincipal, TScopedRoleAssignment>
        where TUserPrincipal : IUserPrincipal, new()
        where TScopedRoleAssignment : IScopedRoleAssignment
    {
        public override TUserPrincipal UserPrincipal
        {
            get
            {
                if (userPrincipal == null || !userPrincipal.Claims.Any())
                    userPrincipal = ValidateClaimsAndBuildUserPrincipal();

                return userPrincipal;
            }
        }
        private TUserPrincipal userPrincipal;

        public UserPrincipalBuilderTyped(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
            : base(httpContextAccessor, scopedRoleAssignmentProvider)
        {
        }
    }
}