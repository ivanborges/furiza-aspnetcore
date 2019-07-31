using AutoMapper;
using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Roles;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.ScopedRoleAssignments;
using Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.Users;
using System;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UsersGetResult>()
                .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.IdentityUserRoles))
                .ForMember(d => d.Claims, opt => opt.MapFrom(s => s.IdentityClaims))
                .ForMember(d => d.LockoutEnd, opt => opt.MapFrom(s => s.LockoutEnd.HasValue ? (DateTime?)s.LockoutEnd.Value.DateTime : null));
            CreateMap<ApplicationUserRole, UsersGetResult.UsersGetResultInnerRole>()
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.IdentityRole.Name));
            CreateMap<ApplicationUserClaim, UsersGetResult.UsersGetResultInnerClaim>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.ClaimType))
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.ClaimValue));

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