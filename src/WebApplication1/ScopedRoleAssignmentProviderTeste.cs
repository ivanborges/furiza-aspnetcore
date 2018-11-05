using Furiza.Base.Core.Identity.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class ScopedRoleAssignmentProviderTeste : IScopedRoleAssignmentProvider
    {
        public Task<IEnumerable<TScopedRoleAssignment>> GetUserScopedRoleAssignmentsAsync<TScopedRoleAssignment>(string username, Guid clientId) where TScopedRoleAssignment : IScopedRoleAssignment
        {
            throw new NotImplementedException();
        }
    }
}