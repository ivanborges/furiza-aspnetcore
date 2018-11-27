using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.ScopedRoleAssignments;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Users;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UsersGetResult>();
            CreateMap<UsersPost, ApplicationUser>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.UserName.Trim().ToLower()));
            CreateMap<ApplicationUserScopedRole, ScopedRoleAssignmentsGetResult>();
        }
    }
}