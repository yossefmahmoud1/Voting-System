namespace VotingSystem.Dtos.Users;

    public record UserResponse
    (
        string Id,
        string Email,
        string FristName,
        string LastName,
        bool IsDisabled,
        IEnumerable<string> Roles
    );


