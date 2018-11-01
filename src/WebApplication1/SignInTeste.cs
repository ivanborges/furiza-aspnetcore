using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace WebApplication1
{
    internal class SignInTeste : ISignInManager
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public SignInTeste(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public async Task<bool> CheckPasswordSignInAsync<TUserPrincipal>(TUserPrincipal userPrincipal, string password) where TUserPrincipal : IUserPrincipal
        {
            var checkIdentity = await signInManager.CheckPasswordSignInAsync(userPrincipal as ApplicationUser, password, false);
            return checkIdentity.Succeeded;
        }
    }
}