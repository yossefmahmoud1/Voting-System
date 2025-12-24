using Hangfire;
using SurveyBasket.Dtos.Common;
using SurveyBasket.Extensions;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Implementation;

namespace SurveyBasket.Services;

public class PollService : IPollService
{
    private readonly IRepository<Poll> _pollRepository;
    private readonly INotifcationService notifcationService;
    private readonly ApplicationDbContext _context;

    public PollService(IRepository<Poll> pollRepository, INotifcationService notifcationService, ApplicationDbContext context)
    {
        _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
        this.notifcationService = notifcationService;
        _context = context;

    }
    public async Task<Result<PaginatedList<Poll>>> GetAllAsync(
        RequestFilters? filters = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        filters ??= new RequestFilters();

        var query = _pollRepository
            .GetQueryable(asNoTracking)
            .ApplyFilters(
                filters,
                p => p.Title,
                p => p.Summary);

        // Default sort (if client didn't send SortColumn)
        if (string.IsNullOrWhiteSpace(filters.SortColumn))
            query = query.OrderByDescending(p => p.Id);

        var paginatedResult = await PaginatedList<Poll>.CreateAsync(
            query,
            filters.PageNumber,
            filters.PageSize,
            cancellationToken);

        return Result.Success(paginatedResult);
    }


    public async Task<Result<IEnumerable<PollResponse>>> GetActive(bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Polls.AsQueryable();

        if (asNoTracking)
            query = query.AsNoTracking();

        var polls = await query
            .Where(p => p.IsPublished && p.StartsAt<=DateOnly.FromDateTime(DateTime.UtcNow)&&p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
            .ProjectToType<PollResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<PollResponse>>(polls);
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
        var existingPoll = await _pollRepository.AnyAsync(x =>x.Title == poll.Title , cancellationToken:cancellationToken);
        if (existingPoll)
            return Result.Fail<Poll>(PollErrors.PollAlreadyExists);
        await _pollRepository.AddAsync(poll, cancellationToken);
        await _pollRepository.SaveChangesAsync(cancellationToken);
        return Result.Success(poll);
    }




    public async Task<Result<Poll>> UpdateAsync(int id, Poll poll, CancellationToken cancellationToken = default)
    {
        // 1️⃣ تأكد إن العنوان مش مكرر
        var isTitleTaken = await _pollRepository.AnyAsync(
            x => x.Title == poll.Title && x.Id != id,
            cancellationToken
        );

        if (isTitleTaken)
            return Result.Fail<Poll>(PollErrors.PollAlreadyExists);

        // 2️⃣ احضر الـ Poll الحالي
        var existingPollResult = await GetAsync(id, cancellationToken);
        if (existingPollResult.IsFailiure)
            return Result.Fail<Poll>(PollErrors.PollNotFound);

        var pollToUpdate = existingPollResult.Value;

        // 3️⃣ حدث البيانات
        pollToUpdate.Title = poll.Title;
        pollToUpdate.Summary = poll.Summary;
        pollToUpdate.IsPublished = poll.IsPublished;
        pollToUpdate.StartsAt = poll.StartsAt;
        pollToUpdate.EndsAt = poll.EndsAt;

        // 4️⃣ حفظ التغييرات
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

        if(poll.IsPublished && poll.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
        {
          BackgroundJob.Enqueue(() =>notifcationService.SendNewPollNotficationsAsync(poll.Id)  );
        }

        return Result.Success(poll);
    }

}