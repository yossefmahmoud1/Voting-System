using System.Data;
using VotingSystem.Dtos.Auth;
using VotingSystem.Dtos.Polls;

namespace VotingSystem.Dtos.Validations
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
