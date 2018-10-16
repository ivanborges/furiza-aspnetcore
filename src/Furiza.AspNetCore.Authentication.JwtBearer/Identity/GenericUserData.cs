using Furiza.Base.Core.Identity.Abstractions;
using System.Collections.Generic;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericUserData : IUserData
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public ICollection<IRoleData> Roles { get; set; }
        public ICollection<IClaimData> Claims { get; set; }
    }
}