using SurveyBasket.Dtos.Votes;

namespace SurveyBasket.Dtos.Validations
{
    public class VoteAnswerValidator:AbstractValidator<VoteAnswerRequest>
    {
        public VoteAnswerValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0).WithMessage(errorMessage: "QuestionId must be greater than 0");
            RuleFor(x => x.answerId)
                .GreaterThan(0).WithMessage(errorMessage: "AnswerId must be greater than 0");
        }

    }
}
