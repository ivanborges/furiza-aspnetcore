using Microsoft.AspNetCore.Identity;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    public interface IPasswordGenerator
    {
        string GenerateRandomPassword(PasswordOptions opts = null);
    }
}