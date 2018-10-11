using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Furiza.AspNetCore.WebApiConfiguration
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public abstract class RootController : ControllerBase
    {
    }
}