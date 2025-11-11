using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult>  Login(LoginDto loginDto , CancellationToken cancellationToken=default)
        {
            var AuthResponse = await authService.LoginAsync(loginDto, cancellationToken);
         return AuthResponse.IsSuccess
                ? Ok(AuthResponse.Value)
                : BadRequest(AuthResponse.Error);
        }


   
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var authResponse = await authService.RegisterAsync(registerDto, cancellationToken);

            if (authResponse is null)
                return BadRequest(new
                {
                    Message = "Registration failed."
                });

            // لو authResponse يحتوي على اقتراحات لاسم مستخدم
            if (authResponse is UsernameTakenResponse takenResponse)
                return BadRequest(takenResponse);

            // أخطاء التسجيل العامة (مثل تكرار الإيميل أو فشل سياسة كلمة المرور)
            if (authResponse is RegistrationErrorResponse errorResponse)
                return BadRequest(errorResponse);

            // لو التسجيل نجح
            return Ok(authResponse);
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
    }
}
