using Microsoft.AspNetCore.Authorization;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Common;
using VotingSystem.PremisonsAuth;

namespace VotingSystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet("")]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> GetAll([FromQuery] RequestFilters? filters, CancellationToken cancellationToken)
    {
        var result = await _pollService.GetAllAsync(filters, asNoTracking: true, cancellationToken);

        return result.IsSuccess
            ? Ok(new
            {
                Items = result.Value.Items.Adapt<IEnumerable<PollResponse>>(),
                PageNumber = result.Value.PageNumber,
                TotalPages = result.Value.TotalPages,
                HasPreviousPage = result.Value.HasPreviousPage,
                HasNextPage = result.Value.HasNextPage
            })
            : result.ToProblem();
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.GetAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Adapt<PollResponse>())
            : result.ToProblem(); 
    }


    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value.Adapt<PollResponse>())
            : result.ToProblem();
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.UpdateAsync(id, request.Adapt<Poll>(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }


        [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(); 
    }

    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Adapt<PollResponse>())
            : result.ToProblem();
    }
    [HttpGet("active")]
    [Authorize(Roles = DefaultRoles.Member)]

    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _pollService.GetActive(
            asNoTracking: true,
            cancellationToken: cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Adapt<IEnumerable<PollResponse>>())
            : result.ToProblem();
    }

}