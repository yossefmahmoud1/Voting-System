using SurveyBasket.Dtos.Roles;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .Length(3, 256).WithMessage("Role name must be between 3 and 256 characters.");

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions are required.")
            .NotEmpty().WithMessage("Permissions cannot be empty.");

        RuleFor(x => x.Permissions)
.Must(x => x.Distinct().Count() == x.Count())
            .WithMessage("Permissions must be unique.")
        .When(x => x.Permissions != null);

        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .WithMessage("Permission value cannot be empty.");
    }
}
