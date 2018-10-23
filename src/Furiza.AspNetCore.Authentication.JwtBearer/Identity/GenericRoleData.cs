using Furiza.Base.Core.Identity.Abstractions;
using System;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericRoleData : IRoleData
    {
        public string Name { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual string CreationUser { get; set; }
    }
}