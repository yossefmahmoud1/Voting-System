using Microsoft.AspNetCore.Authorization;

namespace SurveyBasket.PermissionsAuth
{
    public class PermissionRequirement(string Permission):IAuthorizationRequirement
    {
        public string Permission { get; } = Permission;
    }
}
