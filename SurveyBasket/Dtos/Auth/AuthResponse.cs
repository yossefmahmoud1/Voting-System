namespace SurveyBasket.Dtos.Auth
{
    public record AuthResponse
    (
            string id,
            string? Email,
            string FristName,
            string LastName,
            string Token,
            int ExpiresIn 
        );
    
}
