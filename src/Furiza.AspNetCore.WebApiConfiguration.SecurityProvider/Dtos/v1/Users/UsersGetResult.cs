using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users
{
    public class UsersGetResult
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string HiringType { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public IEnumerable<UsersGetResultInnerClaim> Claims { get; set; }
        public IEnumerable<UsersGetResultInnerRole> Roles { get; set; }

        public class UsersGetResultInnerClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public class UsersGetResultInnerRole
        {
            public string RoleName { get; set; }
        }
    }
}