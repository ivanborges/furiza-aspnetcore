using Furiza.AspNetCore.Authentication.JwtBearer.Identity;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContextTypeless : IUserContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public IUserData UserData
        {
            get
            {
                if (userData == null)
                {
                    var claimsIdentity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity ?? throw new UnauthorizedAccessException();
                    if (!claimsIdentity.Claims.Any())
                        return userData;

                    userData = new GenericUserData()
                    {
                        Roles = new List<GenericRoleData>().Cast<IRoleData>().ToList(),
                        Claims = new List<GenericClaimData>().Cast<IClaimData>().ToList()
                    };
                    
                    foreach (var claim in claimsIdentity.Claims)
                    {
                        var claimShortTypeName = claim.Properties.SingleOrDefault(p => p.Key.Contains("ShortTypeName")).Value;
                        if (string.IsNullOrWhiteSpace(claimShortTypeName))
                            claimShortTypeName = claim.Type;

                        switch (claimShortTypeName)
                        {
                            case (JwtRegisteredClaimNames.Sub):
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
                            case "role":
                                userData.Roles.Add(new GenericRoleData()
                                {
                                    Name = claim.Value
                                });
                                break;
                        }

                        userData.Claims.Add(new GenericClaimData()
                        {
                            Type = claim.Type,
                            Value = claim.Value
                        });
                    }
                }

                return userData;
            }
        }
        private IUserData userData;

        public UserContextTypeless(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
    }
}