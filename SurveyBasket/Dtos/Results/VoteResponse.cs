namespace SurveyBasket.Dtos.Results
{
    public record VoteResponse
    (   string VoterName,
         int VoterDate ,
    IEnumerable<QuestionAnswerResponse> SelectedAnswers
    
    );
     

    
}
