namespace SurveyBasket.Dtos.Results
{
    public record Results
   (
        string Titel,
        IEnumerable<VoteResponse> Votes

        );
}
