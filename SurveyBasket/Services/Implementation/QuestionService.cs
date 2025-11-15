using Microsoft.EntityFrameworkCore;
using SurveyBasket.Dtos.Questions;
using SurveyBasket.Entities.Answers;
using SurveyBasket.Entities.Questions;
using SurveyBasket.Errors;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class QuestionService: IQuestionService
    {
        private readonly IRepository<Question> _questionsrepo;
        private readonly IRepository<Poll> _pollsrepo;

        private readonly ApplicationDbContext _context;

        public QuestionService(
            ApplicationDbContext context,
            IRepository<Question> questions,
            IRepository<Poll> polls)
        {
            _context = context;
            _questionsrepo = questions;
            _pollsrepo = polls;
        }

        public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int PollId, CancellationToken cancellationToken = default)
        {
            // 1) check poll
            var pollExists = await _pollsrepo.AnyAsync(x => x.Id == PollId, cancellationToken);
            if (!pollExists)
                return Result.Fail<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

            // 2) fetch questions WITH answers (using dbcontext not repository)
            var questions = await _context.Questions
                .Where(x => x.pollId == PollId)
                .Include(x => x.Answers)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // 3) map to response
            var response = questions.Adapt<IEnumerable<QuestionResponse>>();

            return Result.Success(response);

        } 
        public async Task<Result<QuestionResponse>> AddAsync( int PollId, QuestionRequest questionRequest, CancellationToken cancellationToken = default)
        {
            // 1) Check if the poll exists before adding a new question
            var PollIsExiests = await _pollsrepo.AnyAsync(
                x => x.Id == PollId,
                cancellationToken: cancellationToken
            );

            if (!PollIsExiests)
            {
                return Result.Fail<QuestionResponse>(PollErrors.PollNotFound);
            }

            // 2) Check if a question with the same content already exists for this poll
            var QuestionsIsExiests = await _questionsrepo.AnyAsync(
                x => x.Content == questionRequest.Content && x.pollId == PollId,
                cancellationToken: cancellationToken
            );

            if (QuestionsIsExiests)
            {
                return Result.Fail<QuestionResponse>(QuestionErros.DublicatedContent);
            }

            // 3) Map incoming DTO (QuestionRequest) to the Question entity
            var question = questionRequest.Adapt<Question>();
            question.pollId = PollId;

         

            // 5) Save the new question to the database
            await _questionsrepo.AddAsync(question, cancellationToken);
            await _questionsrepo.SaveChangesAsync();

            // 6) Return the created question mapped to a response DTO
            return Result.Success(question.Adapt<QuestionResponse>());
        }

        public async Task<Result<QuestionResponse>> GetByIdAsync(
        int PollId,
        int QuestionId,
        CancellationToken cancellationToken = default)
        {
            // Query with projection
            var question = await _context.Questions
                .Where(x => x.pollId == PollId && x.Id == QuestionId)
                .Include(x => x.Answers)
                .ProjectToType<QuestionResponse>()   
                 .AsNoTracking()                     
                .FirstOrDefaultAsync(cancellationToken);

            // Check not found
            if (question == null)
                return Result.Fail<QuestionResponse>(QuestionErros.QuestionlNotFound);

            // Successful
            return Result.Success(question);   // خلاص خلاصنا، مش محتاج Adapt
        }

        public async Task<Result<QuestionResponse>> ToggleStatusAsync(int PollId, int QuestionId, CancellationToken cancellationToken = default)
        {
            var question = await _context.Questions
                .Where(x => x.pollId == PollId && x.Id == QuestionId)
                .Include(x => x.Answers)
                .FirstOrDefaultAsync(cancellationToken);

            if (question is null)
                return Result.Fail<QuestionResponse>(QuestionErros.QuestionlNotFound);

            question.IsActive = !question.IsActive;

            await _questionsrepo.SaveChangesAsync(cancellationToken);

            var response = question.Adapt<QuestionResponse>();
            return Result.Success(response);
        }

        public async Task<Result<QuestionResponse>> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            // 1) Check for duplicated question content in the same poll (except the current question)
            var questionIsExists = await _context.Questions
                .AnyAsync(x => x.pollId == pollId
                    && x.Id != id
                    && x.Content == request.Content,
                    cancellationToken
                );

            if (questionIsExists)
                return Result.Fail<QuestionResponse>(QuestionErros.DublicatedContent);

            // 2) Fetch the question with its answers (needed for update logic)
            var question = await _context.Questions
                .Include(x => x.Answers)
                .SingleOrDefaultAsync(x => x.pollId == pollId && x.Id == id, cancellationToken);

            // 3) If not found return 404 error
            if (question is null)
                return Result.Fail<QuestionResponse>(QuestionErros.QuestionlNotFound);

            // 4) Update question main fields
            question.Content = request.Content;

            // 5) Get current answers from DB
            var currentAnswers = question.Answers.Select(x => x.Content).ToList();

            // 6) Determine which answers are new (in request but not in DB)
            var newAnswers = request.Answers.Except(currentAnswers).ToList();

            // 7) Add the newly created answers
            newAnswers.ForEach(answer =>
            {
                question.Answers.Add(new Answer { Content = answer });
            });

            // 8) Update IsActive for all answers (activate those included in request, deactivate others)
            question.Answers.ToList().ForEach(answer =>
            {
                answer.IsActive = request.Answers.Contains(answer.Content);
            });

            // 9) Save all changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            // 10) Map to response
            var response = question.Adapt<QuestionResponse>();

            // 11) Return success WITH response
            return Result.Success(response);
        }
    }
}
