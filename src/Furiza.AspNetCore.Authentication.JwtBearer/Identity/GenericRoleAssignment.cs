using Furiza.Base.Core.Identity.Abstractions;
using System;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericRoleAssignment : IRoleAssignment
    {
        public Guid? ClientId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}