using SurveyBasket.Abstraction;

namespace SurveyBasket.Errors
{
    public class PollErrors
    {
        public static readonly Error PollNotFound =
           new Error("Poll.Notfound", "No Poll Was Found With The Given Id");
    }
}
