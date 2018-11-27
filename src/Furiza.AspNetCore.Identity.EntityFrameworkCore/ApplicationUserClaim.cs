using Microsoft.AspNetCore.Identity;
using System;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
        public virtual ApplicationUser IdentityUser { get; set; }
    }
}