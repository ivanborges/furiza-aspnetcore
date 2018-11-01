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
    internal abstract class UserPrincipalBuilderBase<TUserPrincipal, TScopedRoleAssignment> : IUserPrincipalBuilder<TUserPrincipal, TScopedRoleAssignment>
        where TUserPrincipal : IUserPrincipal, new()
        where TScopedRoleAssignment : IScopedRoleAssignment
    {
        protected readonly IHttpContextAccessor httpContextAccessor;
        protected readonly IScopedRoleAssignmentProvider scopedRoleAssignmentProvider;

        public abstract TUserPrincipal UserPrincipal { get; }

        public UserPrincipalBuilderBase(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.scopedRoleAssignmentProvider = scopedRoleAssignmentProvider ?? throw new ArgumentNullException(nameof(scopedRoleAssignmentProvider));
        }

        public virtual async Task<IEnumerable<TScopedRoleAssignment>> GetScopedRoleAssignmentsAsync()
        {
            var clientId = UserPrincipal?.Claims?.SingleOrDefault(c => c.Type == FurizaClaimNames.ClientId)?.Value;
            if (!string.IsNullOrWhiteSpace(clientId) && clientId != default(Guid).ToString())
                return (await scopedRoleAssignmentProvider.GetUserScopedRoleAssignmentsAsync<TScopedRoleAssignment>(UserPrincipal.UserName, new Guid(clientId)));
            else
                return default(IEnumerable<TScopedRoleAssignment>);
        }

        protected TUserPrincipal ValidateClaimsAndBuildUserPrincipal()
        {
            var claimsIdentity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity ?? throw new UnauthorizedAccessException();
            if (!claimsIdentity.Claims.Any())
                return default(TUserPrincipal);

            var userPrincipal = new TUserPrincipal()
            {
                Claims = new List<Claim>(),
                RoleAssignments = new List<GenericRoleAssignment>().ToList<IRoleAssignment>()
            };

            foreach (var claim in claimsIdentity.Claims)
            {
                var claimShortTypeName = claim.Properties.SingleOrDefault(p => p.Key.Contains("ShortTypeName")).Value;
                if (string.IsNullOrWhiteSpace(claimShortTypeName))
                    claimShortTypeName = claim.Type;

                switch (claimShortTypeName)
                {
                    case (JwtRegisteredClaimNames.Sub):
                        userPrincipal.UserName = claim.Value;
                        break;
                    case (JwtRegisteredClaimNames.Aud):
                        userPrincipal.Claims.Add(new Claim(FurizaClaimNames.ClientId, claim.Value));
                        break;
                    case (JwtRegisteredClaimNames.GivenName):
                        userPrincipal.FullName = claim.Value;
                        break;
                    case (JwtRegisteredClaimNames.Email):
                        userPrincipal.Email = claim.Value;
                        break;
                    case (FurizaClaimNames.HiringType):
                        userPrincipal.HiringType = claim.Value;
                        break;
                    case (FurizaClaimNames.Company):
                        userPrincipal.Company = claim.Value;
                        break;
                    case (FurizaClaimNames.Department):
                        userPrincipal.Department = claim.Value;
                        break;
                    case (ClaimTypes.Role):
                    case "role":
                        userPrincipal.RoleAssignments.Add(new GenericRoleAssignment() { Role = claim.Value });
                        break;
                    default:
                        userPrincipal.Claims.Add(new Claim(claim.Type, claim.Value));
                        break;
                }
            }

            return userPrincipal;
        }
    }
}