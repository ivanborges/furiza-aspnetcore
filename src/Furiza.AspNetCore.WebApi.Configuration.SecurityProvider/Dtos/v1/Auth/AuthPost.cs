using System;
using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Auth
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