using SurveyBasket.Dtos.User;

namespace SurveyBasket.Dtos.Validations;

    public class ForgetPasswordByEmailRequestValidator:AbstractValidator<ForgetPasswordByEmailRequest>
{
    public ForgetPasswordByEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
        .EmailAddress();

     
    }
}
    
    

