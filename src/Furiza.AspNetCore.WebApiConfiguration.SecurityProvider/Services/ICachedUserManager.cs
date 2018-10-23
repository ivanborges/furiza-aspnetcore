using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    public interface ICachedUserManager
    {
        Task<ApplicationUser> GetUserByUserNameAsync(string username);
    }
}