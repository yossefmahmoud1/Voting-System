using Microsoft.EntityFrameworkCore;
using SurveyBasket.Dtos.Questions;
using SurveyBasket.Dtos.Votes;
using SurveyBasket.Entities;
using SurveyBasket.Entities.Votes;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class VoteService(ApplicationDbContext context, IRepository<Poll> pollsrepo) : IVoteService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IRepository<Poll> _pollsrepo = pollsrepo;

        public async Task<Result> AddAsync(int PollId, string userId, VoteRequest voteRequest, CancellationToken cancellationToken = default)
        {

            // 1) Check if the user has already voted in this poll
            var Hasvote = await _context.Votes
                .AnyAsync(x => x.PollId == PollId && x.UserId == userId, cancellationToken);

            if (Hasvote)
                return Result.Fail(VoteErrors.AlreadyVoted);

            // 2) Check if the poll exists and is currently active (within Start/End dates)
            var PollIsExiests = await _pollsrepo.AnyAsync(
                p => p.Id == PollId
                  && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                  && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow),
                cancellationToken: cancellationToken
            );

            if (!PollIsExiests)
                return Result.Fail(PollErrors.PollNotFound);


            var AvailableQuestions = await _context.Questions
                .Where(q => q.pollId == PollId && q.IsActive)
                .Select(q => q.Id)
                .ToListAsync(cancellationToken);

            if (!voteRequest.Answers.All(a => AvailableQuestions.Contains(a.QuestionId)))
            {
                return Result.Fail(VoteErrors.InvalidQuestions);
            }

            // Validate that all answers belong to their questions and are active
            var answerIds = voteRequest.Answers.Select(a => a.answerId).ToList();
            var validAnswers = await _context.Answers
                .Where(a => answerIds.Contains(a.Id) && a.IsActive)
                .Select(a => new { a.Id, a.QuestionId })
                .ToListAsync(cancellationToken);

            var answerQuestionMap = validAnswers.ToDictionary(a => a.Id, a => a.QuestionId);
            
            foreach (var voteAnswer in voteRequest.Answers)
            {
                if (!answerQuestionMap.ContainsKey(voteAnswer.answerId) ||
                    answerQuestionMap[voteAnswer.answerId] != voteAnswer.QuestionId)
                {
                    return Result.Fail(VoteErrors.InvalidQuestions);
                }
            }

            var vote = new Vote
            {
                PollId = PollId,
                UserId = userId,
                VoteAnswers = voteRequest.Answers.Adapt<List<VoteAnswer>>()
            };

            await _context.Votes.AddAsync(vote, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();

        }
    }
}
