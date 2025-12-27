using Microsoft.AspNetCore.Authorization;
using VotingSystem.Abstraction;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Roles;
using VotingSystem.PremisonsAuth;
using VotingSystem.Services.Implementation;
using VotingSystem.Services.Interfaces;

namespace VotingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController(IRoleService roleService): ControllerBase
    {
        private readonly IRoleService roleService = roleService;

        [HttpGet("")]
        [HasPermission(Permissions.GetRoles)]
        public async Task<IActionResult> GetAll([FromQuery]bool IncludeDiasbeled , CancellationToken cancellationToken )
        {
            var roles= await roleService.GetAllAsync(IncludeDiasbeled, cancellationToken);
            return Ok(roles);

        }
        [HttpGet("{id}")]
        [HasPermission(Permissions.GetRoles)]
        public async Task<IActionResult> GetRoleByid([FromRoute]string Id, CancellationToken cancellationToken)
        {
            var result = await roleService.GetAsync(Id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

        }
        [HttpPost("")]
        [HasPermission(Permissions.AddRoles)]
        public async Task<IActionResult> AddRole([FromBody]RoleRequest roleRequest, CancellationToken cancellationToken)
        {
            var result = await roleService.AddAsync(roleRequest, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

        }
        [HttpPut("{id}")]
        [HasPermission(Permissions.UpdateRoles)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request)
        {
            var result = await roleService.UpdateAsync(id, request);

            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        [HttpPut("{id}/toggle-status")]
        [HasPermission(Permissions.UpdateRoles)]
        public async Task<IActionResult> ToggleStatus([FromRoute] string id)
        {
            var result = await roleService.ToggleStatusAsync(id);

            return result.IsSuccess ? NoContent() : result.ToProblem();
        }

        [HttpPost("{userId}/assign-role")]
        [HasPermission(Permissions.UpdateRoles)]
        public async Task<IActionResult> AssignRole(
            [FromRoute] string userId,
            [FromBody] AssignRoleRequest request,
            CancellationToken cancellationToken)
        {
            var result = await roleService.AssignRoleAsync(userId, request, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToProblem();
        }
    }
}
