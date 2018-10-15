using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebApplication1
{
    internal class EmailSenderTeste : IUserNotifier
    {
        public async Task NotifyNewUserAsync(string emailAddress, string username, string callbackUrl, string password = null)
        {
            await Task.Delay(1000);

            await SendEmailAsync(emailAddress, "Confirm your email",
                $"Hi {username}, blablabla your password is {password}. Confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Task.Delay(1000);
        }
    }
}