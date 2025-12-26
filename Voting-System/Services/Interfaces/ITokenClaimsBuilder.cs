using System.Security.Claims;

namespace VotingSystem.Services.Interfaces
{
    public interface ITokenClaimsBuilder
    {
        Task<List<Claim>> BuildClaimsAsync(Application_User user);
    }
}
