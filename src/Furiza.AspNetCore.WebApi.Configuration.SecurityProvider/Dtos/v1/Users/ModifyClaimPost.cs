using System.ComponentModel.DataAnnotations;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users
{
    public class ModifyClaimPost
    {
        [Required]
        public ModifyClaimOperation? Operation { get; set; }

        [Required]
        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }
    }
}