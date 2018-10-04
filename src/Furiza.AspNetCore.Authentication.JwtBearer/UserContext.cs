using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContext<TUserData, TRoleData, TClaimData> : IUserContext<TUserData, TRoleData, TClaimData>
        where TUserData : IUserData
        where TRoleData : IRoleData
        where TClaimData : IClaimData
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public TUserData UserData
        {
            get
            {
                if (userData == null)
                {
                    var claimsIdentity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity ?? throw new UnauthorizedAccessException();

                    userData = Activator.CreateInstance<TUserData>();

                    var roles = new List<TRoleData>();
                    var claims = new List<TClaimData>();

                    foreach (var claim in claimsIdentity.Claims)
                    {
                        switch (claim.Type)
                        {
                            case (JwtRegisteredClaimNames.UniqueName):
                                userData.UserName = claim.Value;
                                break;
                            case (JwtRegisteredClaimNames.GivenName):
                                userData.FullName = claim.Value;
                                break;
                            case (JwtRegisteredClaimNames.Email):
                                userData.Email = claim.Value;
                                break;
                            case (JwtRegisteredClaimNamesCustom.Company):
                                userData.Company = claim.Value;
                                break;
                            case (JwtRegisteredClaimNamesCustom.Department):
                                userData.Department = claim.Value;
                                break;
                            case (ClaimTypes.Role):
                                var roleData = Activator.CreateInstance<TRoleData>();
                                roleData.Name = claim.Value;
                                roles.Add(roleData);
                                break;
                            default:
                                var claimData = Activator.CreateInstance<TClaimData>();
                                claimData.Type = claim.Type;
                                claimData.Value = claim.Value;
                                claims.Add(claimData);
                                break;
                        }
                    }

                    UserData.Roles = roles.Cast<IRoleData>().ToList();
                    UserData.Claims = claims.Cast<IClaimData>().ToList();
                }

                return userData;
            }
        }
        private TUserData userData;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
    }
}