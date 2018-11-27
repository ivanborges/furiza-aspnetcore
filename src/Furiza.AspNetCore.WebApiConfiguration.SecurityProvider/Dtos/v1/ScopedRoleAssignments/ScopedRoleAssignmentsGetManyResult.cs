using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.ScopedRoleAssignments
{
    public class ScopedRoleAssignmentsGetManyResult
    {
        public IEnumerable<ScopedRoleAssignmentsGetResult> ScopedRoleAssignments { get; set; }
    }
}