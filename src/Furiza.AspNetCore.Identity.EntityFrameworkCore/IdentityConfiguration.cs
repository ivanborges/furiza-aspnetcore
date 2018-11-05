using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityConfiguration
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string DefaultEmailAddress { get; set; }

        public bool EnableMigrations { get; set; } = false;
    }
}