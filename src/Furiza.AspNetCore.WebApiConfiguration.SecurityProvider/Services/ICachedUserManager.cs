using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    public interface ICachedUserManager
    {
        Task<ApplicationUser> GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(string username, Guid? clientId = null);
    }
}