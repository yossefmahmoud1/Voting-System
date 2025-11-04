using Microsoft.AspNetCore.Identity;

namespace SurveyBasket.Entities
{
    public sealed class Application_User:IdentityUser
    {
        public string FristName { get; set; } =string.Empty; 
        public string LastName { get; set; }  = string.Empty;

    }
}
