using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Abstraction;
using SurveyBasket.Dtos.Questions;
using SurveyBasket.Services.Implementation;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
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
            return result.ToProblem( StatusCodes.Status404NotFound);
        }
        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromRoute] int pollId , CancellationToken cancellationToken)
        {
           var result = await questionService.GetAllAsync(pollId, cancellationToken);
            if (result.IsSuccess)
                return Ok( result.Value);
            return result.ToProblem( StatusCodes.Status404NotFound);
        }

        [HttpPost("")]
        public async Task<IActionResult> Add([FromRoute] int pollId , [FromBody] QuestionRequest questionRequest, CancellationToken cancellationToken)
        {

            var result = await questionService.AddAsync(pollId, questionRequest, cancellationToken);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById) , new {pollId , result.Value.Id},result.Value);

            return result.Error == QuestionErrors.DuplicatedContent
                ? result.ToProblem(StatusCodes.Status409Conflict)
                : result.ToProblem(StatusCodes.Status404NotFound);
          
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
                : result.ToProblem(StatusCodes.Status404NotFound);
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
                : result.ToProblem(result.Error == QuestionErrors.DuplicatedContent
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status404NotFound);


        }

    }
}
 