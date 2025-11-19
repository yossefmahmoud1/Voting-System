namespace SurveyBasket.Errors
{
    public class  VoteErrors
    {
        public static readonly Error AlreadyVoted =
           new Error("Vote.AlreadyVoted", "This User Voted Before On This Poll");

        public static readonly Error InvalidQuestions =
         new Error("Vote.InvalidQuestions", "InvalidQuestions");



    }
}
