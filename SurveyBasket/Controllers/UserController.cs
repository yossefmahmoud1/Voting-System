using SurveyBasket.Abstraction.Consts;
using SurveyBasket.PremisonsAuth;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUsersServices usersServices) : ControllerBase
    {
        private readonly IUsersServices usersServices = usersServices; 
        [HttpGet("")]
        [HasPermission(Permissions.GetUsers)]
        public async Task<IActionResult> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var Users = await usersServices.GetAllUsersAsync(cancellationToken);

            return Ok(Users);
                }

        [HttpGet("{id}")]
        [HasPermission(Permissions.GetUsers)]
        public async Task<IActionResult> GetUserDetails([FromRoute] string id, CancellationToken cancellationToken)
        {
            var result = await usersServices.GetUserDetails(id, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status404NotFound);
        }
    }
}
