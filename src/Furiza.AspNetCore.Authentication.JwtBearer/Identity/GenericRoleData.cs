using Furiza.Base.Core.Identity.Abstractions;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericRoleData : IRoleData
    {
        public string Name { get; set; }
    }
}