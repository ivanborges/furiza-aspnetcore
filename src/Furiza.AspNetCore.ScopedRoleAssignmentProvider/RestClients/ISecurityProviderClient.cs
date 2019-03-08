using Refit;
using System.Threading.Tasks;

namespace Furiza.AspNetCore.ScopedRoleAssignmentProvider.RestClients
{
    interface ISecurityProviderClient
    {
        [Get("/api/v1/ScopedRoleAssignments")]
        Task<ScopedRoleAssignmentsGetResult> ScopedRoleAssignmentsGetAsync(ScopedRoleAssignmentsGet scopedRoleAssignmentsGet);
    }
}