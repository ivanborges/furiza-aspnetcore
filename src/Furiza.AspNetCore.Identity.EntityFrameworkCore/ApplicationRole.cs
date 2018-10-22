using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationRole : IdentityRole<Guid>, IRoleData
    {
        public virtual ICollection<ApplicationUserRole> IdentityUserRoles { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual string CreationUser { get; set; }
    }
}