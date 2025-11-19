using Microsoft.EntityFrameworkCore;
using SurveyBasket.Contracts.Results;
using SurveyBasket.Dtos.Results;
using SurveyBasket.Errors;
using SurveyBasket.Persistence;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class ResultService : IResultService
    {
        private readonly ApplicationDbContext _context;

        public ResultService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Dtos.Results.Results>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls
                .Where(x => x.Id == pollId)
                .Include(x => x.Votes)
                    .ThenInclude(v => v.User)
                .Include(x => x.Votes)
                    .ThenInclude(v => v.VoteAnswers)
                        .ThenInclude(va => va.Question)
                .Include(x => x.Votes)
                    .ThenInclude(v => v.VoteAnswers)
                        .ThenInclude(va => va.Answer)
                .FirstOrDefaultAsync(cancellationToken);

            if (poll is null)
                return Result.Fail<Dtos.Results.Results>(PollErrors.PollNotFound);

            var votes = poll.Votes.Select(v => new VoteResponse(
                $"{v.User.FristName} {v.User.LastName}",
                v.SubmitedAt.Day,
                v.VoteAnswers.Select(a => new QuestionAnswerResponse(
                    a.Question.Content,
                    a.Answer.Content
                ))
            )).ToList();

            var response = new Dtos.Results.Results(
                poll.Title,
                votes
            );

            return Result.Success(response);
        }

        public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);

            if (!pollIsExists)
                return Result.Fail<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

            var votesPerDay = await _context.Votes
                .Where(x => x.PollId == pollId)
                .GroupBy(x => new { Date = DateOnly.FromDateTime(x.SubmitedAt) })
                .Select(g => new VotesPerDayResponse(
                    g.Key.Date,
                    g.Count()
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
        }

        public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);

            if (!pollIsExists)
                return Result.Fail<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);

            var votesPerQuestion = await _context.VoteAnswers
                .Where(x => x.Vote.PollId == pollId)
                .GroupBy(x => new { QuestionId = x.QuestionId, QuestionContent = x.Question.Content })
                .Select(g => new VotesPerQuestionResponse(
                    g.Key.QuestionContent,
                    g.GroupBy(va => new { AnswerId = va.AnswerId, AnswerContent = va.Answer.Content })
                        .Select(ag => new VotesPerAnswerResponse(
                            ag.Key.AnswerContent,
                            ag.Count()
                        ))
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);
        }
    }
}
