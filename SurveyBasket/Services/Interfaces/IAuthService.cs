using SurveyBasket.Dtos.Auth;

namespace SurveyBasket.Services.Interfaces
{
    public interface IAuthService
    {
        Task<object?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        Task<AuthResponse?> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }


}
