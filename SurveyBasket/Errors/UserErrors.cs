using Microsoft.AspNetCore.Http;
using SurveyBasket.Abstraction;

namespace SurveyBasket.Errors
{
    public static class UserErrors
    {
        public static readonly Error InvalidCredentials =
            new("User.InvalidCredentials", "Invalid Email or Password", StatusCodes.Status401Unauthorized);

        public static readonly Error EmailNotConfirmed =
            new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status403Forbidden);

        public static readonly Error EmailAlreadyExists =
            new("User.EmailAlreadyExists", "Email already exists", StatusCodes.Status409Conflict);

        public static readonly Error UserNotFound =
            new("User.NotFound", "User not found", StatusCodes.Status404NotFound);

        public static readonly Error InvalidConfirmationToken =
            new("User.InvalidConfirmationToken", "Invalid or expired confirmation token", StatusCodes.Status400BadRequest);

        public static readonly Error EmailAlreadyConfirmed =
            new("User.EmailAlreadyConfirmed", "Email is already confirmed", StatusCodes.Status409Conflict);

        public static Error UsernameTaken(List<string> suggestions) =>
            new UsernameTakenError(suggestions);

        public static Error RegisterFailed(string message) =>
            new("User.RegisterFailed", message, StatusCodes.Status400BadRequest);

        public static readonly Error Unexpected =
            new("User.Unexpected", "Unexpected error while creating user", StatusCodes.Status500InternalServerError);

        public static readonly Error DuplicatedConfirmation =
            new("User.DuplicatedConfirmation", "Confirmation request has already been processed", StatusCodes.Status400BadRequest);
    }
}
