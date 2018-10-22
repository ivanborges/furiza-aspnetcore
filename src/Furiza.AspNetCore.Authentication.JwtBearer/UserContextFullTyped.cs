using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using System;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    internal class UserContextFullTyped<TUserData, TRoleData, TClaimData> : IUserContext<TUserData, TRoleData, TClaimData>
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
                    userData = UserContextHelper.ValidateClaimsAndBuildUserData<TUserData, TRoleData, TClaimData>(httpContextAccessor);                

                return userData;
            }
        }
        private TUserData userData;

        public UserContextFullTyped(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
    }
}