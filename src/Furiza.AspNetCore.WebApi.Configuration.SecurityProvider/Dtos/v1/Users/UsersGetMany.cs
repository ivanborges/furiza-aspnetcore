﻿namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users
{
    public class UsersGetMany
    {
        public string Role { get; set; }
        public string HiringType { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
    }
}