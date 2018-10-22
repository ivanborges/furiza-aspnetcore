using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class ApplicationUserClaim : IdentityUserClaim<Guid>, IClaimData
    {
        public virtual string Type
        {
            get => ClaimType;
            set => ClaimType = value;
        }
        public virtual string Value
        {
            get => ClaimValue;
            set => ClaimValue = value;
        }

        public virtual ApplicationUser IdentityUser { get; set; }

        public virtual DateTime? CreationDate { get; set; }
        public virtual string CreationUser { get; set; }
    }
}