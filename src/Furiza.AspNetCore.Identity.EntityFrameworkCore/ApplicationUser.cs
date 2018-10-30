using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationUser : IdentityUser<Guid>, IUserPrincipal
    {
        public virtual string FullName { get; set; }
        public virtual string HiringType { get; set; }
        public virtual string Company { get; set; }
        public virtual string Department { get; set; }

        public virtual ICollection<Claim> Claims
        {
            get
            {
                return IdentityClaims?.Select(ic => ic.ToClaim()).ToList();
            }
            set
            {
                IdentityClaims = value.Select(c => new ApplicationUserClaim() { ClaimType = c.Type, ClaimValue = c.Value }).ToList();
            }
        }
        public virtual ICollection<ApplicationUserClaim> IdentityClaims { get; set; }

        public virtual ICollection<IRoleAssignment> RoleAssignments
        {
            get
            {
                return IdentityUserRoles?.Select(ur => ur.IdentityRole as IRoleData).ToList();
            }
            set
            {
                IdentityUserRoles = value.Select(r => new ApplicationUserRole() { IdentityRole = new ApplicationRole() { Name = r.Name } }).ToList();
            }
        }
        public virtual ICollection<ApplicationUserRole> IdentityUserRoles { get; set; }

        public virtual DateTime? CreationDate { get; set; }
        public virtual string CreationUser { get; set; }
    }
}