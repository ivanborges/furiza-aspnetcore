using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services
{
    public interface IUserNotifier : IEmailSender
    {
        Task NotifyNewUserAsync(string emailAddress, string username, string callbackUrl, string password = null);

        Task NotifyUserPasswordResetAsync(string emailAddress, string username, string password);
    }
}