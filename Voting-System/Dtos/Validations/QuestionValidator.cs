using VotingSystem.Dtos.Questions;

namespace VotingSystem.Dtos.Validations
{
    public class QuestionValidator :AbstractValidator<QuestionRequest>
    {
        public QuestionValidator() {
            RuleFor(x => x.Content)
                    .NotEmpty()
                    .Length(3, 1000);

            RuleFor(x => x.Answers)
                .NotNull();

            RuleFor(x => x.Answers)
                .Must(x => x.Count > 1)
                .WithMessage("Question Should Have At Least 2 Answers")
                .When(x => x.Answers != null);



            RuleFor(x => x.Answers)
                    .Must(x => x.Distinct().Count() == x.Count)
                    .WithMessage("Cannot Add Dublicated Answers For Same Question")
                            .When(x => x.Answers != null);



        }
    }
}
