namespace VotingSystem.Dtos.Auth;
public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);