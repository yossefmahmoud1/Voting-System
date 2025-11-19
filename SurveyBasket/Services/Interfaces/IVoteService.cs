using SurveyBasket.Dtos.Votes;

namespace SurveyBasket.Services.Interfaces
{
    public interface IVoteService
    {
        Task<Result> AddAsync(int PollId, string userId, VoteRequest voteRequest,CancellationToken cancellationToken = default );

    }
}
