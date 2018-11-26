using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.ScopedRoleAssignmentProvider
{
    public class ScopedRoleAssignmentProviderConfiguration
    {
        [Required]
        public string SecurityProviderApiUrl { get; set; }
    }
}