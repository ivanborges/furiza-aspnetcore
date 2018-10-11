using Furiza.Base.Core.Identity.Abstractions;
using System;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Auth
{
    public class PostResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }

        public PostResult()
        {
        }

        public PostResult(GenerateTokenResult generateTokenResult)
        {
            AccessToken = generateTokenResult.AccessToken;
            RefreshToken = generateTokenResult.RefreshToken;
            Expiration = generateTokenResult.Expiration;
        }
    }
}