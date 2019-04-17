using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users
{
    public class UsersGetManyResult
    {
        public IEnumerable<UsersGetResult> Users { get; set; }        
    }    
}