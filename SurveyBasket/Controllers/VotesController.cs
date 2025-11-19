using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Dtos.Votes;
using SurveyBasket.Extensions;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("api/Polls/{pollId}/Vote")]
    [ApiController]
    public class VotesController(IQuestionService questionService , IVoteService voteService) : ControllerBase
    {
        private readonly IQuestionService _questionService = questionService;
        private readonly IVoteService voteService = voteService;

        [HttpGet("")]
        public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();


            var result = await _questionService.GetAvailable(pollId, userId!, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.Error == VoteErrors.AlreadyVoted
                ? result.ToProblem(StatusCodes.Status409Conflict)
                : result.ToProblem(StatusCodes.Status404NotFound);
        }

        [HttpPost("")]
        public async Task<IActionResult> SubmitVote(
            [FromRoute] int pollId,
            [FromBody] VoteRequest voteRequest,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var result = await voteService.AddAsync (pollId, userId!, voteRequest, cancellationToken);
            if (result.IsSuccess)
                return Created();
            return result.Error == VoteErrors.AlreadyVoted
                ? result.ToProblem(StatusCodes.Status409Conflict)
                : result.ToProblem(StatusCodes.Status404NotFound);
        }
    }
}
