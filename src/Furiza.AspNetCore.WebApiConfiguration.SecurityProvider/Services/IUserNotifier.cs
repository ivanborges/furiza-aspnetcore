using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    public interface IUserNotifier : IEmailSender
    {
        Task NotifyNewUserAsync(string emailAddress, string username, string callbackUrl, string password = null);
    }
}