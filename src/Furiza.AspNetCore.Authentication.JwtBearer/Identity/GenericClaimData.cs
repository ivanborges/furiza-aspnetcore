using Furiza.Base.Core.Identity.Abstractions;
using System;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericClaimData : IClaimData
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual string CreationUser { get; set; }
    }
}