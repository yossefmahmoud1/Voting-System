using SurveyBasket.Dtos.Votes;

namespace SurveyBasket.Dtos.Validations
{
    public class VoteRequestValidator :AbstractValidator<VoteRequest>
    {
     public VoteRequestValidator()
        {
            RuleFor(x => x.Answers).NotEmpty().WithMessage("Answers required.");

            
            RuleForEach(x => x.Answers).SetInheritanceValidator(
                v => v.Add(new VoteAnswerValidator())
                );
        }
    }
}
