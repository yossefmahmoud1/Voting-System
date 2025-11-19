namespace SurveyBasket.Services;

public interface IPollService
{
    Task<Result<IEnumerable<Poll>>> GetAllAsync(bool asNoTracking = false, CancellationToken cancellationToken = default);
    Task<Result<Poll>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Poll>> AddAsync(Poll poll, CancellationToken cancellationToken = default);
    Task<Result<Poll>> UpdateAsync(int id, Poll poll, CancellationToken cancellationToken = default);
    Task<Result<Poll>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Poll>> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PollResponse>>> GetActive(bool asNoTracking = false, CancellationToken cancellationToken = default);

}
