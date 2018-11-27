using Furiza.Base.Core.Identity.Abstractions;
using System.Collections.Generic;
using System.Security.Claims;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericUserPrincipal : IUserPrincipal
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string HiringType { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public ICollection<Claim> Claims { get; set; }
        public ICollection<IRoleAssignment> RoleAssignments { get; set; }
    }
}