using Microsoft.AspNetCore.Authorization;

namespace VotingSystem.PremisonsAuth
{
    public class HasPermissionAttribute(string Permission) : AuthorizeAttribute(Permission)
    {

    }
}
