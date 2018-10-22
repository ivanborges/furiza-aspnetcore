using Furiza.AspNetCore.Authentication.JwtBearer.Identity;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContextUserTyped<TUserData> : IUserContext<TUserData>
        where TUserData : IUserData
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public TUserData UserData
        {
            get
            {
                if (userData == null)
                    userData = UserContextHelper.ValidateClaimsAndBuildUserData<TUserData, GenericRoleData, GenericClaimData>(httpContextAccessor);                

                return userData;
            }
        }
        private TUserData userData;

        public UserContextUserTyped(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
    }
}