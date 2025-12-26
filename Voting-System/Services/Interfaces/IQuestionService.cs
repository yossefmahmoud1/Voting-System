using VotingSystem.Dtos.Common;
using VotingSystem.Dtos.Questions;

namespace VotingSystem.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<Result<QuestionResponse>> AddAsync(int PollId , QuestionRequest questionRequest , CancellationToken cancellationToken=default);
        Task<Result<QuestionResponse>> GetByIdAsync(int PollId , int QuestionId, CancellationToken cancellationToken=default);
        Task<Result<PaginatedList<QuestionResponse>>> GetAllAsync(RequestFilters filters, int PollId ,  CancellationToken cancellationToken=default);
        Task<Result<IEnumerable<QuestionResponse>>> GetAvailable(int PollId , string userId, CancellationToken cancellationToken=default);
        Task<Result<QuestionResponse>> ToggleStatusAsync(int PollId, int QuestionId, CancellationToken cancellationToken=default);
        Task<Result<QuestionResponse>> UpdateAsync(
            int PollId,
            int Id,
            QuestionRequest questionRequest,
            CancellationToken cancellationToken = default
        );
    }
}
 