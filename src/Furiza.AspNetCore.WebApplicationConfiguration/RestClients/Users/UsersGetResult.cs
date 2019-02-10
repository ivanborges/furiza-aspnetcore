using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients.Users
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

        public class UsersGetResultInnerClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}