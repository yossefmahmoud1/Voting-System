using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SurveyBasket.Abstraction;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Dtos.Votes;
using SurveyBasket.Extensions;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("api/Polls/{pollId}/Vote")]
    [ApiController]
    [Authorize(Roles =DefaultRoles.Member)]
    [EnableRateLimiting("concurrency")]
    public class VotesController(IQuestionService questionService , IVoteService voteService) : ControllerBase
    {
        private readonly IQuestionService _questionService = questionService;
        private readonly IVoteService voteService = voteService;

        [HttpGet("")]
        [ResponseCache(Duration =60)]  //just 200
        public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();


            var result = await _questionService.GetAvailable(pollId, userId!, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ToProblem();
            
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
            return result.ToProblem();

        }
    }
}
