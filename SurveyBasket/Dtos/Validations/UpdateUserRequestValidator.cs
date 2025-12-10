using SurveyBasket.Dtos.User;

namespace SurveyBasket.Dtos.Validations
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
        .Length(3, 50);

            RuleFor(x => x.LastName)
        .Length(3, 50);
        }
    }

}
