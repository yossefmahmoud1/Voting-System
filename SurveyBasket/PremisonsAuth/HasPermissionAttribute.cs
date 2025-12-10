using Microsoft.AspNetCore.Authorization;

namespace SurveyBasket.PremisonsAuth
{
    public class HasPermissionAttribute(string Permission) : AuthorizeAttribute(Permission)
    {

    }
}
