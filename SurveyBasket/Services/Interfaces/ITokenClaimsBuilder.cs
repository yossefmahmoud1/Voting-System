using System.Security.Claims;

namespace SurveyBasket.Services.Interfaces
{
    public interface ITokenClaimsBuilder
    {
        Task<List<Claim>> BuildClaimsAsync(Application_User user);
    }
}
