using System;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1
{
    public class PostResult
    {
        public Guid Id { get; }
        public string Code { get; }

        public PostResult(Guid id)
        {
            Id = id;
        }

        public PostResult(Guid id, string code)
        {
            Id = id;
            Code = code;
        }
    }
}