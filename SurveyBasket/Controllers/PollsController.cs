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
        var result = await _pollService.GetAllAsync(asNoTracking: true, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Adapt<IEnumerable<PollResponse>>())
            : result.ToProblem(StatusCodes.Status500InternalServerError);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.GetAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Adapt<PollResponse>())
            : result.ToProblem(StatusCodes.Status404NotFound);
    }


    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value.Adapt<PollResponse>())
            : result.ToProblem(StatusCodes.Status409Conflict);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.UpdateAsync(id, request.Adapt<Poll>(), cancellationToken);

        return result.IsSuccess
            ? NoContent() // ✅ في حالة النجاح نرجع 204 NoContent (صح REST)
            : result.ToProblem(result.Error == PollErrors.PollNotFound
                ? StatusCodes.Status404NotFound   // ✅ لو البولي غير موجود
                : StatusCodes.Status409Conflict);  // ✅ لو الاسم مكرر
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(StatusCodes.Status404NotFound);
    }

    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Adapt<PollResponse>())
            : result.ToProblem(StatusCodes.Status404NotFound);
    }
}