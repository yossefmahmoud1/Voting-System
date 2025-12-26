namespace VotingSystem.Dtos.Auth
{
    public record ResetPasswordRequest
    (
        string Email,
        string Code,
        string NewPassword



        );
}
