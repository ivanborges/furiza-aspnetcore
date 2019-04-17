using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services
{
    public interface ICachedUserManager
    {
        Task<ApplicationUser> GetUserByUserNameAndFilterRoleAssignmentsByClientIdAsync(string username, Guid clientId);
        Task RemoveUserByUserNameAsync(string username);
    }
}