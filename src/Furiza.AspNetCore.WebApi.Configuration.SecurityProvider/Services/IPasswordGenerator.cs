using Microsoft.AspNetCore.Identity;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services
{
    public interface IPasswordGenerator
    {
        string GenerateRandomPassword(PasswordOptions opts = null);
    }
}