namespace SurveyBasket.Dtos.Roles;

    public record RoleDetailsResponse
    (
        string id,
        string Name,
        bool IsDeleted,
        IEnumerable<string> Permissions
    )
    ;
