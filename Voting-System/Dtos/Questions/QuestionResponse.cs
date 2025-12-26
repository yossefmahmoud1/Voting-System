
using VotingSystem.Dtos.Answers;

namespace VotingSystem.Dtos.Questions

{
    public record QuestionResponse(
        int Id,
        string Content,
        bool IsActive,
        IEnumerable<AnswerResponse> Answers
    );
}
