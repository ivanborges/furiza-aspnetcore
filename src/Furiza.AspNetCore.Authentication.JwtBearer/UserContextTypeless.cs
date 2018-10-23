using Furiza.AspNetCore.Authentication.JwtBearer.Identity;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;

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
                    userData = UserContextHelper.ValidateClaimsAndBuildUserData<GenericUserData, GenericRoleData, GenericClaimData>(httpContextAccessor);                

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