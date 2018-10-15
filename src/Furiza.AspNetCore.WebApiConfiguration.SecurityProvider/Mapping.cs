using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, GetResult>();
            CreateMap<Post, ApplicationUser>();
        }
    }
}