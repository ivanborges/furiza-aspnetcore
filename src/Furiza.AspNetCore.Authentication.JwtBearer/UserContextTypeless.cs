using Furiza.AspNetCore.Authentication.JwtBearer.Identity;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContextTypeless : UserContextBase<IUserWallet, GenericScopedRoleAssignment>, IUserContext
    {
        public override IUserWallet UserWallet
        {
            get
            {
                if (userWallet == null)
                    userWallet = ValidateClaimsAndBuildUserWallet();

                return userWallet;
            }
        }
        private IUserWallet userWallet;

        public UserContextTypeless(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
            : base(httpContextAccessor, scopedRoleAssignmentProvider)
        {
        }

        //public async Task<IEnumerable<IScopedRoleAssignment>> GetScopedRoleAssignmentsAsync()
        //{
        //    var clientId = UserWallet?.RoleAssignments?.FirstOrDefault()?.ClientId;
        //    if (clientId.HasValue && clientId.Value != default(Guid))
        //        return await scopedRoleAssignmentProvider.GetUserScopedRoleAssignmentsAsync(UserWallet.UserName, clientId.Value);
        //    else
        //        return default(IEnumerable<IScopedRoleAssignment>);
        //}
    }
}