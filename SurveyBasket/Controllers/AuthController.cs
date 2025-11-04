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
            if (AuthResponse == null)
                return Unauthorized("Invalid Email or Password");
            return Ok(AuthResponse);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var authResponse = await authService.RegisterAsync(registerDto, cancellationToken);
            if (authResponse == null)
                return BadRequest("User already exists or registration failed.");

            return Ok(authResponse);
        }

    }
}
