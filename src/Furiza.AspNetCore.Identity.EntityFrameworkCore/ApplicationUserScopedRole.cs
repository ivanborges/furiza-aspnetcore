using System;
using Furiza.Base.Core.Identity.Abstractions;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationUserScopedRole : IScopedRoleAssignment
    {
        public Guid ClientId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Scope { get; set; }
    }
}