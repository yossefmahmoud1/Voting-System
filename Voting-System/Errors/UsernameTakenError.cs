using VotingSystem.Abstraction;

namespace VotingSystem.Errors
{
    public record UsernameTakenError(
        IReadOnlyList<string> Suggestions
    ) : Error(
        "User.UsernameExists",
        "Username already exists",
        StatusCodes.Status409Conflict
    );
}
