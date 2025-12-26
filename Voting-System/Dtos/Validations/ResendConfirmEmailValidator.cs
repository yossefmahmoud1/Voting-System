namespace VotingSystem.Dtos.Validations
{
    public class ResendConfirmEmailValidator : AbstractValidator<ResendConfirmEmailDto>
    {
        public ResendConfirmEmailValidator()
        {
            RuleFor(x => x.Email).NotEmpty()
                .EmailAddress();
         
        }
    }
}