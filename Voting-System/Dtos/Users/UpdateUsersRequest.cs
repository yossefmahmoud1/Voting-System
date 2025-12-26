namespace VotingSystem.Dtos.Users;

    public record UpdateUsersRequest
    (
        string Email,
        string FristName,
        string LastName,
  
        IList<string> Roles
    );

