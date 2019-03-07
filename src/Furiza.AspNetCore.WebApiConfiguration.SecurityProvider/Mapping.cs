using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.Roles;
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
                       
            CreateMap<ApplicationRole, RolesGetResult>()
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Name.Trim().ToLower()));
            CreateMap<RolesPost, ApplicationRole>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.RoleName.Trim().ToLower()));
        }
    }
}