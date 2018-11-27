using Furiza.Base.Core.Identity.Abstractions;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    public interface ISignInManager
    {
        Task<bool> CheckPasswordSignInAsync<TUserPrincipal>(TUserPrincipal userPrincipal, string password)
            where TUserPrincipal : IUserPrincipal;
    }
}