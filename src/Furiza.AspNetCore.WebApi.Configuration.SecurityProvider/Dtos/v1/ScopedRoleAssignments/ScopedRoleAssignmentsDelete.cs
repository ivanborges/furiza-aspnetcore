using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.ScopedRoleAssignments
{
    public class ScopedRoleAssignmentsDelete
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string RoleName { get; set; }

        [Required]
        public string Scope { get; set; }
    }
}