﻿namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Dtos.v1.ScopedRoleAssignments
{
    public class ScopedRoleAssignmentsGetResult
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Scope { get; set; }
    }
}