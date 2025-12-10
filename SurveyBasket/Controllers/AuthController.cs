using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Dtos.User;
using SurveyBasket.Errors;
using SurveyBasket.Services.Implementation;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth(IAuthService authService, ILogger<AuthService> logger) : ControllerBase
    {
        private readonly IAuthService authService = authService;
        private readonly ILogger<AuthService> logger = logger;

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);
            var AuthResponse = await authService.LoginAsync(loginDto, cancellationToken);
            return AuthResponse.IsSuccess
                   ? Ok(AuthResponse.Value)
                   : BadRequest(AuthResponse.Error);
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var result = await authService.RegisterAsync(registerDto, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            // Handle username taken error with suggestions
            if (result.Error is UsernameTakenError usernameError)
            {
                var response = new UsernameTakenResponse(
                    "Username already exists",
                    usernameError.Suggestions.ToList()
                );
                return BadRequest(response);
            }

            // Handle other registration errors
            var errorResponse = new RegistrationErrorResponse(result.Error.Message);
            return BadRequest(errorResponse);
        }


        [HttpPost("Refresh")]

        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshtokenrequest, CancellationToken cancellationToken = default)
        {
            var authResponse = await authService.RefreshTokenAsync(refreshtokenrequest.Token, refreshtokenrequest.RefreshToken, cancellationToken);
            if (authResponse is null)
                return BadRequest("Invalid refresh token.");
            return Ok(authResponse);
        }

        [HttpPost("Revoke")]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest revokeTokenRequest, CancellationToken cancellationToken = default)
        {
            var result = await authService.RevokeRefreshTokenAsync(revokeTokenRequest.RefreshToken, cancellationToken);
            if (!result)
                return BadRequest("Invalid or already revoked refresh token.");
            return Ok(new { Message = "Refresh token revoked successfully." });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto dto, CancellationToken ct)
        {
            var result = await authService.ConfirmEmailAsync(dto, ct);
            return result.IsSuccess ? Ok(new { Message = "Email confirmed successfully." }) : BadRequest(result.Error);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgetPassword(
      ForgetPasswordByEmailRequest request)
        {
            var result = await authService.SendResetPasswordCodeAsync(request.Email);

            return result.IsSuccess
                ? Ok(new { Message = "If the email exists, a password reset link has been sent." })
                : BadRequest(new { Error = result.Error.Message });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordRequest request
   )
        {
            var result = await authService.ResetPassWordAsync(request);

            return result.IsSuccess
                ? Ok(new { Message = "Password has been reset successfully." })
                : result.ToProblem(result.Error.StatusCode);
        }

        
    }
}
