using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApplicationConfiguration
{
    public class reCaptchaConfiguration
    {
        [Required]
        public string SiteKey { get; set; }

        [Required]
        public string SecretKey { get; set; }
    }
}