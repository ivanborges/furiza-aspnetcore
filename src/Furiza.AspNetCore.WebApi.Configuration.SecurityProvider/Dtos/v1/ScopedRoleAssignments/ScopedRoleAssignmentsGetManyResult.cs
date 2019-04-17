using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Dtos.v1.ScopedRoleAssignments
{
    public class ScopedRoleAssignmentsGetManyResult
    {
        public IEnumerable<ScopedRoleAssignmentsGetResult> ScopedRoleAssignments { get; set; }
    }
}