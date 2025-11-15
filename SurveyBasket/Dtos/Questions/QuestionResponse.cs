
using SurveyBasket.Dtos.Answers;

namespace SurveyBasket.Dtos.Questions

{
    public record QuestionResponse(
        int Id,
        string Content,
        bool IsActive,
        IEnumerable<AnswerResponse> Answers
    );
}
