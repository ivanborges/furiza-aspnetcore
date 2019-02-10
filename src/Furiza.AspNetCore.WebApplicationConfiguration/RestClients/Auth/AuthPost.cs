using System;

namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients.Auth
{
    public class AuthPost
    {
        public GrantType? GrantType { get; set; }
        public Guid? ClientId { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string RefreshToken { get; set; }
    }

    public enum GrantType
    {
        Password,
        RefreshToken
    }
}