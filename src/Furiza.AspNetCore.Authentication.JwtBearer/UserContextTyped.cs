using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContextTyped<TUserWallet, TScopedRoleAssignment> : UserContextBase<TUserWallet, TScopedRoleAssignment>, IUserContext<TUserWallet, TScopedRoleAssignment>
        where TUserWallet : IUserWallet
        where TScopedRoleAssignment : IScopedRoleAssignment
    {
        public override TUserWallet UserWallet
        {
            get
            {
                if (userWallet == null)
                    userWallet = ValidateClaimsAndBuildUserWallet();

                return userWallet;
            }
        }
        private TUserWallet userWallet;

        public UserContextTyped(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
            : base(httpContextAccessor, scopedRoleAssignmentProvider)
        {
        }
    }
}