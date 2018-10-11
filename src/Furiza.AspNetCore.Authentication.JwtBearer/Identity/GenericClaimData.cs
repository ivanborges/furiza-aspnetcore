using Furiza.Base.Core.Identity.Abstractions;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Identity
{
    internal class GenericClaimData : IClaimData
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}