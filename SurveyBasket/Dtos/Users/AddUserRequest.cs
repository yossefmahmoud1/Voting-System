namespace SurveyBasket.Dtos.Users;

    public record AddUserRequest
    (
        string Email,
        string FristName, 
        string LastName,
        string UserName,
        string Password,
        IList<string> Roles
    );

