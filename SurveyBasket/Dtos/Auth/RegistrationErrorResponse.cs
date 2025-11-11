namespace SurveyBasket.Dtos.Auth;

public class RegistrationErrorResponse
{
    public RegistrationErrorResponse(string message)
    {
        Message = message;
    }

    public string Message { get; set; }
}


