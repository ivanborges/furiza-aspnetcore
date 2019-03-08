using System;

namespace Furiza.AspNetCore.ScopedRoleAssignmentProvider.RestClients
{
    public class ScopedRoleAssignmentsGet
    {
        public Guid ClientId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Scope { get; set; }
    }
}