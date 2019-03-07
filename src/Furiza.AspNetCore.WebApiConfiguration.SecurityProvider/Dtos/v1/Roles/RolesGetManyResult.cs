using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Roles
{
    public class RolesGetManyResult
    {
        public IEnumerable<RolesGetResult> Roles { get; set; }
    }
}