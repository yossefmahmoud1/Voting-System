using SurveyBasket.Abstraction;

namespace SurveyBasket.Errors
{
    public static class QuestionErrors
    {
        public static readonly Error QuestionNotFound =
            new("Question.NotFound", "No Question Was Found With The Given Id", StatusCodes.Status404NotFound);

        public static readonly Error DuplicatedContent =
            new("Question.DuplicatedContent", "Another question with the same content already exists", StatusCodes.Status409Conflict);
    }
}
