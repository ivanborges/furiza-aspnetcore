using System;
using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Auth
{
    public class AuthPost
    {
        [Required]
        public GrantType? GrantType { get; set; }

        [Required]
        public Guid? ClientId { get; set; }

        public string User { get; set; }
        public string Password { get; set; }
        public string RefreshToken { get; set; }
    }
}