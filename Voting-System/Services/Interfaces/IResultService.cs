using VotingSystem.Dtos.Results;

namespace VotingSystem.Services.Interfaces
{
    public interface IResultService
    {
        Task<Result<Dtos.Results.Results>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<Contracts.Results.VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<Contracts.Results.VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default);
    }
}
