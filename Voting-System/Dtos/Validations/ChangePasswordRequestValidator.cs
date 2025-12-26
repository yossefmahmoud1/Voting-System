using VotingSystem.Dtos.User;

namespace VotingSystem.Dtos.Validations
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .Matches(RegaxPatterns.Password)
                .WithMessage("Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from current password.");
        }


    }


}
