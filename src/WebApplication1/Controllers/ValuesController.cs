using Furiza.AspNetCore.WebApiConfiguration;
using Furiza.Base.Core.Domain.BR.Exceptions;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [ApiVersion("1.0")]
    public class ValuesController : RootController
    {
        private readonly IUserContext userContext;
        private readonly ILogger logger;

        public ValuesController(ILoggerFactory loggerProvider, IUserContext userContext)
        {
            this.userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            logger = loggerProvider.CreateLogger<ValuesController>();
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BadRequestError), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public ActionResult<string> Get(int id)
        {
            logger.LogDebug("id de entrada: " + id);

            if (id == 1)
                throw new InvalidOperationException("ah vsf vai.");
            else if (id == 2)
                throw new CpfInvalidoException();

            return Ok(id + 1);
        }

        // POST api/values
        [HttpPost]
        //[Authorize(Roles = "Superuser")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(BadRequestError), 406)]
        public ActionResult Post([FromBody] DtoTeste teste)
        {
            var tst = teste;
            return Ok(userContext.UserData);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
