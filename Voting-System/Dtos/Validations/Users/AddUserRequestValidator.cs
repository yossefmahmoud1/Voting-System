using VotingSystem.Dtos.Users;

namespace VotingSystem.Dtos.Validations.Users
{
    public class AddUserRequestValidator : AbstractValidator<AddUserRequest>
    {
        public AddUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email format.");

            RuleFor(x => x.FristName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .Length(3, 24)
                .WithMessage("First name must be between 3 and 24 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .Length(3, 24)
                .WithMessage("Last name must be between 3 and 24 characters.");

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .Matches(RegaxPatterns.Password)
                .WithMessage(
                    "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character."
                );

            RuleFor(x => x.Roles)
                .NotNull()
                .WithMessage("Roles are required.")
                .NotEmpty()
                .WithMessage("At least one role must be provided.");

            RuleFor(x => x.Roles)
                .Must(r => r.Distinct().Count() == r.Count)
                .WithMessage("Roles must be unique.")
                .When(x => x.Roles != null);
        }
    }
}
