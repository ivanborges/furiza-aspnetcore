using Furiza.Base.Core.Identity.Abstractions;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Services
{
    public interface ISignInManager
    {
        Task<bool> CheckPasswordSignInAsync<TUserPrincipal>(TUserPrincipal userPrincipal, string password)
            where TUserPrincipal : IUserPrincipal;
    }
}