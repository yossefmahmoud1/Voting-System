using VotingSystem.Abstraction;

namespace VotingSystem.Errors
{
    public static class PollErrors
    {
        public static readonly Error PollNotFound =
            new("Poll.NotFound", "No Poll Was Found With The Given Id", StatusCodes.Status404NotFound);

        public static readonly Error PollAlreadyExists =
            new("Poll.AlreadyExists", "A poll with the same title already exists", StatusCodes.Status409Conflict);
    }
}
