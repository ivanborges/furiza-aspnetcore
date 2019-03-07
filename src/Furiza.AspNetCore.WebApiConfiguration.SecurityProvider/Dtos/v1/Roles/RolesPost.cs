using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Roles
{
    public class RolesPost
    {
        [Required]
        public string RoleName { get; set; }
    }
}