using Furiza.Base.Core.Identity.Abstractions;
using System;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Auth
{
    public class AuthPostResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }

        public AuthPostResult()
        {
        }

        public AuthPostResult(GenerateTokenResult generateTokenResult)
        {
            AccessToken = generateTokenResult.AccessToken;
            RefreshToken = generateTokenResult.RefreshToken;
            Expiration = generateTokenResult.Expiration;
        }
    }
}