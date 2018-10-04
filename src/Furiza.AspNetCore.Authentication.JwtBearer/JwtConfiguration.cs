using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.Authentication.JwtBearer
{
    public class JwtConfiguration
    {
        [Required]
        public string Issuer { get; set; }

        [Required]
        public string Audience { get; set; }

        public int ExpirationInMinutes { get; set; } = 180;
    }
}