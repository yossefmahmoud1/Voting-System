using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Services.OptionsPattern
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

        public int expiresInHours { get; init; } = 1; // المدة الافتراضية 1 ساعة


    }
}
