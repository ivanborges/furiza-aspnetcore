using Furiza.AspNetCore.Authentication.JwtBearer.Identity;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal abstract class UserContextBase<TUserWallet, TScopedRoleAssignment> //: IUserContext<TUserWallet, TScopedRoleAssignment>
        where TUserWallet : IUserWallet
        where TScopedRoleAssignment : IScopedRoleAssignment
    {
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly IScopedRoleAssignmentProvider scopedRoleAssignmentProvider;

        public abstract TUserWallet UserWallet { get; }

        public UserContextBase(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.scopedRoleAssignmentProvider = scopedRoleAssignmentProvider ?? throw new ArgumentNullException(nameof(scopedRoleAssignmentProvider));
        }

        public virtual async Task<IEnumerable<TScopedRoleAssignment>> GetScopedRoleAssignmentsAsync()
        {
            var clientId = UserWallet?.RoleAssignments?.FirstOrDefault()?.ClientId;
            if (clientId.HasValue && clientId.Value != default(Guid))
                return (await scopedRoleAssignmentProvider.GetUserScopedRoleAssignmentsAsync(UserWallet.UserName, clientId.Value)).Cast<TScopedRoleAssignment>();
            else
                return default(IEnumerable<TScopedRoleAssignment>);
        }

        protected TUserWallet ValidateClaimsAndBuildUserWallet()
        {
            var claimsIdentity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity ?? throw new UnauthorizedAccessException();
            if (!claimsIdentity.Claims.Any())
                return default(TUserWallet);

            var userWallet = Activator.CreateInstance<TUserWallet>();
            userWallet.Claims = new List<Claim>();
            userWallet.RoleAssignments = new List<GenericRoleAssignment>();

            foreach (var claim in claimsIdentity.Claims)
            {
                var claimShortTypeName = claim.Properties.SingleOrDefault(p => p.Key.Contains("ShortTypeName")).Value;
                if (string.IsNullOrWhiteSpace(claimShortTypeName))
                    claimShortTypeName = claim.Type;

                switch (claimShortTypeName)
                {
                    case (JwtRegisteredClaimNames.Sub):
                        userWallet.UserName = claim.Value;
                        break;
                    case (JwtRegisteredClaimNames.GivenName):
                        userWallet.FullName = claim.Value;
                        break;
                    case (JwtRegisteredClaimNames.Email):
                        userWallet.Email = claim.Value;
                        break;
                    case (FurizaClaimNames.HiringType):
                        userWallet.HiringType = claim.Value;
                        break;
                    case (FurizaClaimNames.Company):
                        userWallet.Company = claim.Value;
                        break;
                    case (FurizaClaimNames.Department):
                        userWallet.Department = claim.Value;
                        break;
                    case (ClaimTypes.Role):
                    case "role":
                        userWallet.RoleAssignments.Add(new GenericRoleAssignment() { Role = claim.Value });
                        break;
                    default:
                        userWallet.Claims.Add(new Claim(claim.Type, claim.Value));
                        break;
                }
            }

            return userWallet;
        }
    }
}