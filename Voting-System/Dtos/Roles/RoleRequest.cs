using System.Collections;

namespace VotingSystem.Dtos.Roles
{
    public record RoleRequest
    (
        string Name,

        IEnumerable<string> Permissions
        );

}
