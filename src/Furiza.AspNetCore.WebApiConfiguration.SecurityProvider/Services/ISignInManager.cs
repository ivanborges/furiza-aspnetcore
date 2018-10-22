using Furiza.Base.Core.Identity.Abstractions;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services
{
    public interface ISignInManager<TUserData> where TUserData : IUserData
    {
        Task<bool> CheckPasswordSignInAsync(TUserData user, string password);
    }
}