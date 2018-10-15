using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users
{
    public class GetManyResult
    {
        public IEnumerable<GetResult> Users { get; set; }        
    }    
}