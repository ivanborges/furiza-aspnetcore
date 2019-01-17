using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.Authentication.JwtBearer.Cookies
{
    public class AuthenticationConfigurationJwt
    {
        [Required]
        public string Issuer { get; set; }

        [Required]
        public string Audience { get; set; }

        [Required]
        public string Secret { get; set; }
    }
}