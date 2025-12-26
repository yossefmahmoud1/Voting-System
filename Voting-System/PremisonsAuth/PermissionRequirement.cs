using Microsoft.AspNetCore.Authorization;

namespace VotingSystem.PermissionsAuth
{
    public class PermissionRequirement(string Permission):IAuthorizationRequirement
    {
        public string Permission { get; } = Permission;
    }
}
