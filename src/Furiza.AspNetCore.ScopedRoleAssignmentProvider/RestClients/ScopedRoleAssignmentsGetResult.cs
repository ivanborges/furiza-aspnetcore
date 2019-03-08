using System.Collections.Generic;

namespace Furiza.AspNetCore.ScopedRoleAssignmentProvider.RestClients
{
    public class ScopedRoleAssignmentsGetResult
    {
        public IEnumerable<ScopedRoleAssignmentsGetResultInnerScopedRoleAssignment> ScopedRoleAssignments { get; set; }

        public class ScopedRoleAssignmentsGetResultInnerScopedRoleAssignment
        {
            public string UserName { get; set; }
            public string Role { get; set; }
            public string Scope { get; set; }
        }
    }
}