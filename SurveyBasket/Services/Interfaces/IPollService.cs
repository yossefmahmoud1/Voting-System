using SurveyBasket.Dtos.Common;

namespace SurveyBasket.Services;

public interface IPollService
{
    Task<Result<PaginatedList<Poll>>> GetAllAsync(RequestFilters? filters = null, bool asNoTracking = false, CancellationToken cancellationToken = default);
    Task<Result<Poll>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Poll>> AddAsync(Poll poll, CancellationToken cancellationToken = default);
    Task<Result<Poll>> UpdateAsync(int id, Poll poll, CancellationToken cancellationToken = default);
    Task<Result<Poll>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Poll>> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PollResponse>>> GetActive(bool asNoTracking = false, CancellationToken cancellationToken = default);

}
