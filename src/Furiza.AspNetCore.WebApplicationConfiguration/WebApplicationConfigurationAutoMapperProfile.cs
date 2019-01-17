using AutoMapper;
using Furiza.AspNetCore.Authentication.JwtBearer.Cookies.Services;
using Furiza.AspNetCore.WebApplicationConfiguration.RestClients;

namespace Furiza.AspNetCore.WebApplicationConfiguration
{
    public class WebApplicationConfigurationAutoMapperProfile : Profile
    {
        public WebApplicationConfigurationAutoMapperProfile()
        {
            CreateMap<AuthPostResult, RefreshTokenResult>();
        }
    }
}