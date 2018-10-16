using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users
{
    public class ConfirmEmailGet
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Token { get; set; }
    }
}