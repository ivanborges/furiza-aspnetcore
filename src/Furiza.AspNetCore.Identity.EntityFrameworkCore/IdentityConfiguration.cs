using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityConfiguration
    {
        [Required]
        public string ConnectionString { get; set; }

        public bool EnableMigrations { get; set; } = false;
        public bool EnableInitializer { get; set; } = false;
    }
}