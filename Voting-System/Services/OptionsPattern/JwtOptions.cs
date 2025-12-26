using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Services.OptionsPattern
{
    public class JwtOptions
    {
        [Required]
        public string secretKey { get; init; } = string.Empty;
        [Required]

        public string issuer { get; init; } = string.Empty;
        [Required]

        public string audience { get; init; } = string.Empty;
        [Range(1 , int.MaxValue)]

        public int expiresInHours { get; init; } = 1; // ????? ?????????? 1 ????


    }
}
