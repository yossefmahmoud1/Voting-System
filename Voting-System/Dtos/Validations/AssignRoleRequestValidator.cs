using FluentValidation;
using VotingSystem.Dtos.Roles;

namespace VotingSystem.Dtos.Validations
{
    public class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
    {
        public AssignRoleRequestValidator()
        {
            // Must provide either RoleName or Permissions, but not both
            RuleFor(x => x)
                .Must(x => (!string.IsNullOrWhiteSpace(x.RoleName) && (x.Permissions == null || !x.Permissions.Any())) ||
                           (string.IsNullOrWhiteSpace(x.RoleName) && x.Permissions != null && x.Permissions.Any()))
                .WithMessage("Either RoleName or Permissions must be provided, but not both.");

            // Validate permissions when provided
            RuleForEach(x => x.Permissions)
                .NotEmpty()
                .When(x => x.Permissions != null && x.Permissions.Any())
                .WithMessage("Permission value cannot be empty.");

            RuleFor(x => x.Permissions)
                .Must(x => x == null || x.Distinct().Count() == x.Count())
                .When(x => x.Permissions != null && x.Permissions.Any())
                .WithMessage("Permissions must be unique.");
        }
    }
}

