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

        protected UserPrincipalBuilderBase(IHttpContextAccessor httpContextAccessor,
            IScopedRoleAssignmentProvider scopedRoleAssignmentProvider)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.scopedRoleAssignmentProvider = scopedRoleAssignmentProvider ?? throw new ArgumentNullException(nameof(scopedRoleAssignmentProvider));
        }

        public virtual Guid GetCurrentClientId() => new Guid(UserPrincipal.Claims.Single(c => c.Type == FurizaClaimNames.ClientId).Value);

        public virtual async Task<IEnumerable<TScopedRoleAssignment>> GetScopedRoleAssignmentsAsync() => await scopedRoleAssignmentProvider.GetUserScopedRoleAssignmentsAsync<TScopedRoleAssignment>(UserPrincipal.UserName, GetCurrentClientId());

        protected TUserPrincipal ValidateClaimsAndBuildUserPrincipal()
        {
            var userPrincipal = new TUserPrincipal()
            {
                Claims = new List<Claim>(),
                RoleAssignments = new List<GenericRoleAssignment>().ToList<IRoleAssignment>()
            };

            var claimsIdentity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
            if (claimsIdentity == null || !claimsIdentity.Claims.Any())
                return userPrincipal;

            if (claimsIdentity.Claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud) == null)
                throw new InvalidOperationException("ClaimsIdentity does not contain a valid claim for Audience which represents the ClientId.");

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