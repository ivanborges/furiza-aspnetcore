using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users
{
    public class UsersGetByEmail
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}