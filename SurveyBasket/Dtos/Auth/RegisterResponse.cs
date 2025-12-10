namespace SurveyBasket.Dtos.Auth;

public record RegisterResponse
(
    string UserId,
    string ConfirmationCode
);

