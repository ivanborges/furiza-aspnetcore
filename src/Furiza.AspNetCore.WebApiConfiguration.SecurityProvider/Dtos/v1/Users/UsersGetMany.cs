using Furiza.Base.Core.Identity.Abstractions;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users
{
    public class UsersGetMany
    {
        public Role? Role { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
    }
}