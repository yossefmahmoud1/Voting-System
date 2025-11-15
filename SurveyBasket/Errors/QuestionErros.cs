namespace SurveyBasket.Errors
{
    public class QuestionErros
    {
        public static readonly Error QuestionlNotFound =
           new Error("Question.Notfound", "No Question Was Found With The Given Id");
        public static readonly Error DublicatedContent =
              new Error("Question.DublicatedContent", "Another Question With The Same Content Is Already Exists");

    }
}
