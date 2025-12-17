using System.Collections;

namespace SurveyBasket.Dtos.Roles
{
    public record RoleRequest
    (
        string Name,

        IEnumerable<string> Permissions
        );

}
