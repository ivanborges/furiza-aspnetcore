﻿using System;

namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients
{
    public class AuthPostResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
    }
}