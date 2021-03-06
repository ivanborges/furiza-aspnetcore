﻿using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users
{
    public class UsersPost
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string HiringType { get; set; }

        [Required]
        public string Company { get; set; }

        [Required]
        public string Department { get; set; }

        public string Password { get; set; }
        public bool GeneratePassword { get; set; }
    }
}