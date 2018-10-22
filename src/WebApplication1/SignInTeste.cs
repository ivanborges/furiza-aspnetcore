using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace WebApplication1
{
    internal class SignInTeste : ISignInManager<ApplicationUser>
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public SignInTeste(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public async Task<bool> CheckPasswordSignInAsync(ApplicationUser user, string password)
        {
            var checkIdentity = await signInManager.CheckPasswordSignInAsync(user, password, false);
            return checkIdentity.Succeeded;
        }
    }
}