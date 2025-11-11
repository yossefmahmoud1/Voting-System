namespace SurveyBasket.Dtos.Auth;
public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);