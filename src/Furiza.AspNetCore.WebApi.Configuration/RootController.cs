using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Furiza.AspNetCore.WebApi.Configuration
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public abstract class RootController : ControllerBase
    {
    }
}