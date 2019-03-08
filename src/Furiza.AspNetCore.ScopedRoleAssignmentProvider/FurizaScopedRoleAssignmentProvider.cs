using Furiza.AspNetCore.ScopedRoleAssignmentProvider.RestClients;
using Furiza.Base.Core.Identity.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.ScopedRoleAssignmentProvider
{
    internal class FurizaScopedRoleAssignmentProvider : IScopedRoleAssignmentProvider
    {
        private readonly ISecurityProviderClient securityProviderClient;

        public FurizaScopedRoleAssignmentProvider(ISecurityProviderClient securityProviderClient)
        {
            this.securityProviderClient = securityProviderClient ?? throw new ArgumentNullException(nameof(securityProviderClient));
        }

        public async Task<IEnumerable<TScopedRoleAssignment>> GetUserScopedRoleAssignmentsAsync<TScopedRoleAssignment>(string username, Guid clientId) where TScopedRoleAssignment : IScopedRoleAssignment
        {
            var scopedRoleAssignmentsGet = new ScopedRoleAssignmentsGet()
            {
                ClientId = clientId,
                UserName = username
            };
            var scopedRoleAssignmentsGetResult = await securityProviderClient.ScopedRoleAssignmentsGetAsync(scopedRoleAssignmentsGet);

            var tScopedRoleAssignments = scopedRoleAssignmentsGetResult.ScopedRoleAssignments.Select(sra => 
            {
                var tsra = Activator.CreateInstance<TScopedRoleAssignment>();
                tsra.ClientId = clientId;
                tsra.UserName = sra.UserName;
                tsra.Role = sra.Role;
                tsra.Scope = sra.Scope;

                return tsra;
            });

            return tScopedRoleAssignments;
        }
    }
}