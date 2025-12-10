using SurveyBasket.Dtos.Auth;

namespace SurveyBasket.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<AuthResponse>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        Task<AuthResponse?> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<Result> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken cancellationToken = default);
        Task<Result> ResendConfirmationEmail(ResendConfirmEmailDto dto, CancellationToken cancellationToken = default);

        Task<Result> ResetPassWordAsync(ResetPasswordRequest resetPasswordRequest);
        Task<Result> SendResetPasswordCodeAsync(string Email);


    }
}
