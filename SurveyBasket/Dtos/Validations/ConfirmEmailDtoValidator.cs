namespace SurveyBasket.Dtos.Validations
{
    public class ConfirmEmailDtoValidator: AbstractValidator<ConfirmEmailDto>
    {
        public ConfirmEmailDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Code)
            .NotEmpty();
        }
    }
    
    }

