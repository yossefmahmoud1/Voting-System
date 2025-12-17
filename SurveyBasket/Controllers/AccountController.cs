using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Dtos.User;
using SurveyBasket.Extensions;
using SurveyBasket.Services.Implementation;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("me")]
    [ApiController]
    [Authorize]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        private readonly IAccountService accountService = accountService;


        [HttpGet("")]
        public async Task<IActionResult> GetUserProfileAsync(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();


            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await accountService.GetUserProfileAsync(userId);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return Problem(statusCode: result.Error.StatusCode, title: result.Error.Message);
            }
       
        }




        [HttpPut("Update-Profile")]
        public async Task<IActionResult> UpdateProfile(
    [FromBody] UpdateUserRequest request,
    CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await accountService.UpdateUserProfileAsync(userId, request, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : Problem(statusCode: result.Error.StatusCode, title: result.Error.Message);
        }









        [HttpPut("Change-Password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken)
        {
            

            var userId = User.GetUserId();

           

            var result = await accountService.ChangePasswordAsync(userId!,changePasswordRequest,cancellationToken);

            return result.IsSuccess
                ? NoContent()
        : result.ToProblem(result.Error.StatusCode);
        }
    }

}

