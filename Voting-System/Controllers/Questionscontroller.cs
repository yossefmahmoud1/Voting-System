using Microsoft.AspNetCore.Authorization;
using VotingSystem.Abstraction;
using VotingSystem.Dtos.Common;
using VotingSystem.Dtos.Questions;
using VotingSystem.Services.Implementation;
using VotingSystem.Services.Interfaces;

namespace VotingSystem.Controllers
{
    [Route("api/Polls/{pollId}/[controller]")]
    [ApiController]
    [Authorize]
    public class Questionscontroller(IQuestionService questionService) : ControllerBase
    {
        private readonly IQuestionService questionService = questionService;
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int pollId, [FromRoute] int id , CancellationToken cancellationToken)
        {
            var result = await questionService.GetByIdAsync(pollId,  id, cancellationToken);
            if (result.IsSuccess)
                return Ok( result.Value);
            return result.ToProblem();
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromRoute] int pollId, [FromQuery] RequestFilters filters, CancellationToken cancellationToken)
        {
           var result = await questionService.GetAllAsync(filters, pollId, cancellationToken);
            if (result.IsSuccess)
                return Ok( result.Value);
            return result.ToProblem();
        }

        [HttpPost("")]
        public async Task<IActionResult> Add([FromRoute] int pollId , [FromBody] QuestionRequest questionRequest, CancellationToken cancellationToken)
        {

            var result = await questionService.AddAsync(pollId, questionRequest, cancellationToken);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById) , new {pollId , result.Value.Id},result.Value);

            return result.ToProblem();

        }

        [HttpPut("{Id}/ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(
        [FromRoute] int pollId,
        [FromRoute] int Id,
        CancellationToken cancellationToken)
        {
            var result = await questionService.ToggleStatusAsync(pollId, Id, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(
    int pollId,
    int id,
    [FromBody] QuestionRequest request,
    CancellationToken cancellationToken)
        {
            // call service
            var result = await questionService.UpdateAsync(pollId, id, request, cancellationToken);


            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();


        }

    }
}
 