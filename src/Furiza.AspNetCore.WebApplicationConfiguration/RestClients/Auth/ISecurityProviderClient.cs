using Refit;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients.Auth
{
    public interface ISecurityProviderClient
    {
        [Post("/api/v1/Auth")]
        Task<AuthPostResult> AuthAsync(AuthPost authPost);
    }
}