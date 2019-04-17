using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Roles;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.ScopedRoleAssignments;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users;
using System.Linq;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UsersGetResult>()
                .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.IdentityUserRoles.Select(ur => ur.IdentityRole).Select(r => new { RoleName = r.Name })));
            CreateMap<UsersPost, ApplicationUser>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.UserName.Trim().ToLower()));

            CreateMap<ApplicationUserScopedRole, ScopedRoleAssignmentsGetResult>();
                       
            CreateMap<ApplicationRole, RolesGetManyResult.RolesGetManyResultInnerRole>()
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Name.Trim().ToLower()));
            CreateMap<RolesPost, ApplicationRole>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.RoleName.Trim().ToLower()));
        }
    }
}