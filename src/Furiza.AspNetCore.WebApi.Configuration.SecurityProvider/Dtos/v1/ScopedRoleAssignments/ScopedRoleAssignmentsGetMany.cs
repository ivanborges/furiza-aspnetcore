using System;
using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.ScopedRoleAssignments
{
    public class ScopedRoleAssignmentsGetMany
    {
        [Required]
        public Guid? ClientId { get; set; }

        public string UserName { get; set; }
        public string Role { get; set; }
        public string Scope { get; set; }
    }
}