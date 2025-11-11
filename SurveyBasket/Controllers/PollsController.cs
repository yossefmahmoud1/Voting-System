using Microsoft.AspNetCore.Authorization;

namespace SurveyBasket.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var pollsResult = await _pollService.GetAllAsync(asNoTracking: true, cancellationToken);

        if (pollsResult.IsFailiure)
            return BadRequest(pollsResult.Error);

        var response = pollsResult.Value.Adapt<IEnumerable<PollResponse>>();

        return Ok(response);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var pollResult = await _pollService.GetAsync(id, cancellationToken);

        if (pollResult.IsFailiure)
            return pollResult.ToProblem(StatusCodes.Status400BadRequest  );


        var response = pollResult.Value.Adapt<PollResponse>();

        return Ok(response);
    }


    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request,
        CancellationToken cancellationToken)
    {
        var newPollResult = await _pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);

        if (newPollResult.IsFailiure)
            return BadRequest(newPollResult.Error);

        var response = newPollResult.Value.Adapt<PollResponse>();
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request,
        CancellationToken cancellationToken)
    {
        var updateResult = await _pollService.UpdateAsync(id, request.Adapt<Poll>(), cancellationToken);

        if (updateResult.IsFailiure)
            return NotFound(updateResult.Error);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var deleteResult = await _pollService.DeleteAsync(id, cancellationToken);

        if (deleteResult.IsFailiure)
            return NotFound(deleteResult.Error);

        return NoContent();

    }

    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var toggleResult = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        if (toggleResult.IsFailiure)
            return NotFound(toggleResult.Error);

        return NoContent();
    }
}