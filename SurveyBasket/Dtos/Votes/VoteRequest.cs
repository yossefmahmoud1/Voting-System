namespace SurveyBasket.Dtos.Votes;

    public record VoteRequest
   (
        IEnumerable<VoteAnswerRequest> Answers 
);
      

