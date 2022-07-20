using Microsoft.AspNetCore.Authorization;

namespace GrpcAuth.Server.Permission
{
    internal class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}