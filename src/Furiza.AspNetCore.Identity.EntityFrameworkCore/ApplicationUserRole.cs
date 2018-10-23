using Microsoft.AspNetCore.Identity;
using System;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser IdentityUser { get; set; }
        public virtual ApplicationRole IdentityRole { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual string CreationUser { get; set; }
    }
}