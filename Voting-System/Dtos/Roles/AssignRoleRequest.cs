namespace VotingSystem.Dtos.Roles
{
    public record AssignRoleRequest
    {
        public string? RoleName { get; init; }
        public IEnumerable<string>? Permissions { get; init; }
    }
}

