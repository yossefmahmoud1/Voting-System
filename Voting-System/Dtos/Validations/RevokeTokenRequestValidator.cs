using FluentValidation;
using VotingSystem.Dtos.Auth;

namespace VotingSystem.Dtos.Validations;
public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

