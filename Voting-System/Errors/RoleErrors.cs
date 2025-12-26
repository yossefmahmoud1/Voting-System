namespace VotingSystem.Errors
{
    public class RoleErrors
    {
        public static readonly Error RoleNotFound =
            new("Role.NotFound", "No Role Was Found With The Given Id", StatusCodes.Status404NotFound);

        public static readonly Error RoleAlreadyExists =
            new("Role.AlreadyExists", "A Role with the same title already exists", StatusCodes.Status409Conflict);
        public static readonly Error InvalidPermissions =
          new(
              "Role.InvalidPermissions",
              "One or more permissions are invalid",
              StatusCodes.Status400BadRequest
          );
        public static readonly Error InvalidRoles =
          new(
              "Role.InvalidRoles",
              "One or more Roles are invalid",
              StatusCodes.Status400BadRequest
          );
    }
}
