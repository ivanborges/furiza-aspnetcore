using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationUserRole : IdentityUserRole<Guid>, IRoleAssignment
    {
        public virtual Guid ClientId { get; set; }

        public virtual string UserName
        {
            get
            {
                return IdentityUser?.UserName;
            }
            set
            {
                IdentityUser = new ApplicationUser() { UserName = value };
            }
        }
        public virtual ApplicationUser IdentityUser { get; set; }

        public virtual string Role
        {
            get
            {
                return IdentityRole?.Name;
            }
            set
            {
                IdentityRole = new ApplicationRole() { Name = value };
            }
        }
        public virtual ApplicationRole IdentityRole { get; set; }
    }
}