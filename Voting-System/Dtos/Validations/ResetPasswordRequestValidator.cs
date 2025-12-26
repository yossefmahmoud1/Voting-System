namespace VotingSystem.Dtos.Validations
{
    public class ResetPasswordRequestValidator: AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
            RuleFor(x => x.Code)
           .NotEmpty();


            RuleFor(x => x.NewPassword)
                .NotEmpty()
            .Matches(RegaxPatterns.Password) 
            .WithMessage("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");


        }
    }


}