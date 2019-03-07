using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users
{
    public class UsersGetByEmail
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}