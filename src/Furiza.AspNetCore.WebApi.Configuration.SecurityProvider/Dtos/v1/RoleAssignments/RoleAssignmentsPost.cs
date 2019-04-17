using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.RoleAssignments
{
    public class RoleAssignmentsPost
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
