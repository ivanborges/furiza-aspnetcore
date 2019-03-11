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

        [Get("/api/v1/Users/byemail")]
        Task<UsersGetResult> GetByEmailAsync(UsersGetByEmail usersGetByEmail);

        [Post("/api/v1/Users/ChangePassword")]
        Task ChangePasswordPostAsync(ChangePasswordPost changePasswordPost);

        [Post("/api/v1/Users/{userName}/ResetPassword")]
        Task ResetPasswordPostAsync(string userName);
    }
}