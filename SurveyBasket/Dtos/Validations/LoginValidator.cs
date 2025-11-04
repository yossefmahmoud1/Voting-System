using System.Data;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Dtos.Polls;

namespace SurveyBasket.Dtos.Validations
{

    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x=> x.Email)
            .NotEmpty()
            .EmailAddress();

            RuleFor(x => x.PassWord)
                .NotEmpty();


        }
    }


}
