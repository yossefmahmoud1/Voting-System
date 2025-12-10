using SurveyBasket.Abstraction;

namespace SurveyBasket.Errors
{
    public static class VoteErrors
    {
        public static readonly Error AlreadyVoted =
            new("Vote.AlreadyVoted", "This user has already voted on this poll", StatusCodes.Status409Conflict);

        public static readonly Error InvalidQuestions =
            new("Vote.InvalidQuestions", "One or more questions are invalid", StatusCodes.Status400BadRequest);
    }
}
