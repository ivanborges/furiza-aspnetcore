using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Roles
{
    public class RolesGetManyResult
    {
        public IEnumerable<RolesGetManyResultInnerRole> Roles { get; set; }

        public class RolesGetManyResultInnerRole
        {
            public string RoleName { get; set; }
        }
    }
}