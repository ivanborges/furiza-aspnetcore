﻿using Furiza.AspNetCore.Identity.EntityFrameworkCore;
using Furiza.Base.Core.Exceptions.Serialization;
using Furiza.Base.Core.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Controllers.v1
{
    [ApiVersion("1.0")]
    public class ClientsController : RootController
    {
        private readonly IUserPrincipalBuilder userPrincipalBuilder;
        private readonly IdentityInitializer identityInitializer;

        public ClientsController(IUserPrincipalBuilder userPrincipalBuilder,
            IdentityInitializer identityInitializer)
        {
            this.userPrincipalBuilder = userPrincipalBuilder ?? throw new ArgumentNullException(nameof(userPrincipalBuilder));
            this.identityInitializer = identityInitializer ?? throw new ArgumentNullException(nameof(identityInitializer));
        }

        [AllowAnonymous]
        [HttpPost("{clientId}/Initialize")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(InternalServerError), 500)]
        public async Task<IActionResult> InitializePostAsync(Guid clientId)
        {
            if (userPrincipalBuilder.UserPrincipal.Claims.SingleOrDefault(c => c.Type == FurizaClaimNames.ClientId) == null)
                userPrincipalBuilder.UserPrincipal.Claims.Add(new Claim(FurizaClaimNames.ClientId, clientId.ToString()));

            await identityInitializer.InitializeUsersAsync();

            return NoContent();
        }
    }
}