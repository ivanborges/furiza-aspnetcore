using Furiza.Base.Core.Identity.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.ScopedRoleAssignmentProvider
{
    internal class FurizaScopedRoleAssignmentProvider : IScopedRoleAssignmentProvider
    {
        public Task<IEnumerable<TScopedRoleAssignment>> GetUserScopedRoleAssignmentsAsync<TScopedRoleAssignment>(string username, Guid clientId) where TScopedRoleAssignment : IScopedRoleAssignment
        {
            throw new NotImplementedException();
        }
    }
}