using SurveyBasket.Dtos.Auth;

namespace SurveyBasket.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<AuthResponse?> LoginAsync(LoginDto loginDto , CancellationToken cancellationToken = default);
    }
}
