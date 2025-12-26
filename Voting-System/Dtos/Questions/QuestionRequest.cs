namespace VotingSystem.Dtos.Questions
{
    public record QuestionRequest
   ( 
        string Content ,
        List<string> Answers
    );
}
