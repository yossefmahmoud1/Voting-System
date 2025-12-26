namespace VotingSystem.Dtos.Votes;

    public record VoteRequest
   (
        IEnumerable<VoteAnswerRequest> Answers 
);
      

