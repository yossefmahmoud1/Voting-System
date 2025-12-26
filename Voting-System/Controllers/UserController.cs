using VotingSystem.Abstraction;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Common;
using VotingSystem.Dtos.Users;
using VotingSystem.PremisonsAuth;
using VotingSystem.Services.Implementation;
using VotingSystem.Services.Interfaces;

namespace VotingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUsersServices usersServices) : ControllerBase
    {
        private readonly IUsersServices usersServices = usersServices; 
        [HttpGet("")]
        [HasPermission(Permissions.GetUsers)]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] RequestFilters? filters, CancellationToken cancellationToken)
        {
            var Users = await usersServices.GetAllUsersAsync(filters, cancellationToken);

            return Ok(Users);
                }

        [HttpGet("{id}")]
        [HasPermission(Permissions.GetUsers)]
        public async Task<IActionResult> GetUserDetails([FromRoute] string id, CancellationToken cancellationToken)
        {
            var result = await usersServices.GetUserDetails(id, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }



        [HttpPost("")]
        [HasPermission(Permissions.AddUsers)]
        public async Task<IActionResult> AddUser(
    [FromBody] AddUserRequest addUserRequest,
    CancellationToken cancellationToken)
        {
            var result = await usersServices.AddUserAsync(addUserRequest, cancellationToken);

            return result.IsSuccess
      ? Created(string.Empty, result.Value)
      : result.ToProblem();

        }


        [HttpPut("{id}")]
        [HasPermission(Permissions.UpdateUsers)]
        public async Task<IActionResult> UpdateUser(
        [FromRoute] string id,
        [FromBody] UpdateUsersRequest request,
        CancellationToken cancellationToken)
        {
            var result = await usersServices.UpdateUserAsync(id, request, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }


        [HttpPut("{id}/toggle-status")]
        [HasPermission(Permissions.UpdateUsers)]
        public async Task<IActionResult> ToggleStatus([FromRoute] string id)
        {
            var result = await usersServices.ToggleStatus(id);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        [HttpPut("{id}/unlock")]
        [HasPermission(Permissions.UpdateUsers)]
        public async Task<IActionResult> Unlock([FromRoute] string id)
        {
            var result = await usersServices.Unlock(id);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }
    };





}
