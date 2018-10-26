﻿using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContextHelper
    {
        protected UserContextHelper()
        {
        }

        public static TUserWallet ValidateClaimsAndBuildUserData<TUserWallet, TRoleAssignment>(IHttpContextAccessor httpContextAccessor)
            where TUserWallet : IUserWallet
            where TRoleAssignment : IRoleAssignment
        {
            var claimsIdentity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity ?? throw new UnauthorizedAccessException();
            if (!claimsIdentity.Claims.Any())
                return default(TUserWallet);

            var userWallet = Activator.CreateInstance<TUserWallet>();
            userWallet.Claims = new List<Claim>();
            var roleAssignments = new List<TRoleAssignment>();

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
                        var roleData = Activator.CreateInstance<TRoleAssignment>();
                        roleData.Role = claim.Value;
                        roleAssignments.Add(roleData);
                        break;
                    default:
                        userWallet.Claims.Add(new Claim(claim.Type, claim.Value));
                        break;
                }
            }

            userWallet.RoleAssignments = roleAssignments.Cast<IRoleAssignment>().ToList();

            return userWallet;
        }
    }
}