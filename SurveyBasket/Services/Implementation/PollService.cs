using SurveyBasket.Repositeryes.Interfaces;

namespace SurveyBasket.Services;

public class PollService : IPollService
{
    private readonly IRepository<Poll> _pollRepository;

    public PollService(IRepository<Poll> pollRepository)
    {
        _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
    }
    public async Task<Result<IEnumerable<Poll>>> GetAllAsync(bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var polls = await _pollRepository.GetAllAsync(asNoTracking, cancellationToken);
        return Result.Success(polls);
    }



    public async Task<Result<Poll>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _pollRepository.GetByIdAsync(id, cancellationToken);
        return poll is not null?
            Result.Success(poll) :
            Result.Fail<Poll>(PollErrors.PollNotFound);
    }

    public async Task<Result<Poll>> AddAsync(Poll poll, CancellationToken cancellationToken = default)
    {
        await _pollRepository.AddAsync(poll, cancellationToken);
        await _pollRepository.SaveChangesAsync(cancellationToken);
        return Result.Success(poll);
    }





    public async Task<Result<Poll>> UpdateAsync(int id, Poll poll, CancellationToken cancellationToken = default)
    {
        var currentPollResult = await GetAsync(id, cancellationToken);

        if (currentPollResult.IsFailiure)
            return Result.Fail<Poll>(PollErrors.PollNotFound);

        var pollToUpdate = currentPollResult.Value;
        pollToUpdate.Title = poll.Title;
        pollToUpdate.Summary = poll.Summary;
        pollToUpdate.IsPublished = poll.IsPublished;
        pollToUpdate.StartsAt = poll.StartsAt;
        pollToUpdate.EndsAt = poll.EndsAt;

        _pollRepository.Update(pollToUpdate);
        await _pollRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(pollToUpdate);
    }


    public async Task<Result<Poll>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var pollResult = await GetAsync(id, cancellationToken);

        if (pollResult.IsFailiure)
            return Result.Fail<Poll>(PollErrors.PollNotFound);

        var poll = pollResult.Value;

        _pollRepository.Remove(poll);
        await _pollRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(poll);
    }


    public async Task<Result<Poll>> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var pollResult = await GetAsync(id, cancellationToken);

        if (pollResult.IsFailiure)
            return Result.Fail<Poll>(PollErrors.PollNotFound);

        var poll = pollResult.Value;
        poll.IsPublished = !poll.IsPublished;

        _pollRepository.Update(poll);
        await _pollRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(poll);
    }

}