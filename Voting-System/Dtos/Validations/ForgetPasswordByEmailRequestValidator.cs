using VotingSystem.Dtos.User;

namespace VotingSystem.Dtos.Validations;

    public class ForgetPasswordByEmailRequestValidator:AbstractValidator<ForgetPasswordByEmailRequest>
{
    public ForgetPasswordByEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
        .EmailAddress();

     
    }
}
    
    

