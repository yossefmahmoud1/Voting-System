using FluentValidation;
using SurveyBasket.Dtos.Auth;

namespace SurveyBasket.Dtos.Validations;
public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

