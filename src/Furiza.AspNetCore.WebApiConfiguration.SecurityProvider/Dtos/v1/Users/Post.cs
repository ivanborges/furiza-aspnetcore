using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users
{
    public class Post
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Company { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public bool? GeneratePassword { get; set; }
    }
}