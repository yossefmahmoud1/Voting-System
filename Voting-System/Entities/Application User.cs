using Microsoft.AspNetCore.Identity;

namespace VotingSystem.Entities
{
    public sealed class Application_User:IdentityUser
    {
        public string FristName { get; set; } =string.Empty; 
        public string LastName { get; set; }  = string.Empty;
        public bool IsDisabled { get; set; } = false;

        public List <RefreshToken> refreshTokens { get; set; } = [];

    }
}
