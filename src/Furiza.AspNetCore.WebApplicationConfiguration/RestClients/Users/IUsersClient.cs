using Refit;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients.Users
{
    public interface IUsersClient
    {
        [Get("/api/v1/Users")]
        Task<UsersGetAllResult> GetAllAsync(UsersGetAll usersGetAll);

        [Get("/api/v1/Users/{userName}")]
        Task<UsersGetResult> GetAsync(string userName);
    }
}