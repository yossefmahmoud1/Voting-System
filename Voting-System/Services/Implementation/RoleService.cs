using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Roles;
using VotingSystem.Services.Interfaces;

namespace VotingSystem.Services.Implementation
{
    public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext applicationDbContext, UserManager<Application_User> userManager) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> roleManager = roleManager;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;
        private readonly UserManager<Application_User> userManager = userManager;

        public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool IncludeDiasbeled = false, CancellationToken cancellationToken = default)
        {
            var Roles = await roleManager.Roles.Where(x =>
        !x.IsDefault &&
        (IncludeDiasbeled  || !x.IsDeleted)
    )

                        .ProjectToType<RoleResponse>()
                        .ToListAsync();

            return Roles;
        }

        public async Task<Result<RoleDetailsResponse>> GetAsync(string Id, CancellationToken cancellationToken = default)
        {
            if (await roleManager.FindByIdAsync(Id) is not { } role)
                return Result.Fail<RoleDetailsResponse>(RoleErrors.RoleNotFound);
            var permissions = await roleManager.GetClaimsAsync(role);
            var response = new RoleDetailsResponse(
                 role.Id,
                 role.Name!,
                 role.IsDeleted,
                 permissions.Select(x => x.Value)
             );
            return Result.Success(response);

        }

        public async Task<Result<RoleDetailsResponse>> AddAsync(RoleRequest request, CancellationToken cancellationToken = default)
        {
            var RoleIsExists = await roleManager.RoleExistsAsync(request.Name);
            if (RoleIsExists)
                return Result.Fail<RoleDetailsResponse>(RoleErrors.RoleAlreadyExists);
            var AllowedPermissions = Permissions.GetAllPermissions();
            if (request.Permissions.Except(AllowedPermissions).Any())
                return Result.Fail<RoleDetailsResponse>(RoleErrors.InvalidPermissions);
            var role = new ApplicationRole
            {
                Name = request.Name,
                ConcurrencyStamp = Guid.NewGuid().ToString()



            };
            var createResult = await roleManager.CreateAsync(role);
            if (createResult.Succeeded)
            {
                var claims = request.Permissions.Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id
                }

                );
                await applicationDbContext.AddRangeAsync(claims);
                await applicationDbContext.SaveChangesAsync(cancellationToken);
                var response = new RoleDetailsResponse(
           role.Id,
           role.Name!,
           role.IsDeleted,
           request.Permissions
       );

                return Result.Success(response);
            }

            var error = createResult.Errors.First();

            return Result.Fail<RoleDetailsResponse>(new Error(
                error.Code,
                error.Description,
                StatusCodes.Status400BadRequest
            ));


        }
        public async Task<Result> UpdateAsync(string id, RoleRequest request)
        {
            var roleIsExists = await roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);

            if (roleIsExists)
                return Result.Fail<RoleDetailsResponse>(RoleErrors.RoleAlreadyExists);

            if (await roleManager.FindByIdAsync(id) is not { } role)
                return Result.Fail<RoleDetailsResponse>(RoleErrors.RoleNotFound);

            var allowedPermissions = Permissions.GetAllPermissions();

            if (request.Permissions.Except(allowedPermissions).Any())
                return Result.Fail<RoleDetailsResponse>(RoleErrors.InvalidPermissions);

            role.Name = request.Name;

            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                var currentPermissions = await applicationDbContext.RoleClaims
                    .Where(x => x.RoleId == id && x.ClaimType == Permissions.Type)
                    .Select(x => x.ClaimValue)
                    .ToListAsync();

                var newPermissions = request.Permissions.Except(currentPermissions)
                    .Select(x => new IdentityRoleClaim<string>
                    {
                        ClaimType = Permissions.Type,
                        ClaimValue = x,
                        RoleId = role.Id
                    });

                var removedPermissions = currentPermissions.Except(request.Permissions);

                await applicationDbContext.RoleClaims
                    .Where(x => x.RoleId == id && removedPermissions.Contains(x.ClaimValue))
                .ExecuteDeleteAsync();


                await applicationDbContext.AddRangeAsync(newPermissions);
                await applicationDbContext.SaveChangesAsync();

                return Result.Success();
            }

            var error = result.Errors.First();

            return Result.Fail<RoleDetailsResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ToggleStatusAsync(string id)
        {
            if (await roleManager.FindByIdAsync(id) is not { } role)
                return Result.Fail<RoleDetailsResponse>(RoleErrors.RoleNotFound);

            role.IsDeleted = !role.IsDeleted;

            await roleManager.UpdateAsync(role);

            return Result.Success();
        }

        public async Task<Result> AssignRoleAsync(string userId, AssignRoleRequest request, CancellationToken cancellationToken = default)
        {
            // 1️⃣ Check user existence
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Fail(UserErrors.UserNotFound);

            // 2️⃣ If RoleName is provided, assign the role
            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                // Check if role exists
                var role = await roleManager.FindByNameAsync(request.RoleName);
                if (role is null)
                    return Result.Fail(RoleErrors.RoleNotFound);

                // Check if user already has this role
                var userRoles = await userManager.GetRolesAsync(user);
                if (!userRoles.Contains(request.RoleName, StringComparer.OrdinalIgnoreCase))
                {
                    var result = await userManager.AddToRoleAsync(user, request.RoleName);
                    if (!result.Succeeded)
                    {
                        var error = result.Errors.First();
                        return Result.Fail(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
                    }
                }

                // Remove any direct permission claims when assigning a role
                var existingPermissionClaims = await applicationDbContext.UserClaims
                    .Where(x => x.UserId == userId && x.ClaimType == Permissions.Type)
                    .ToListAsync(cancellationToken);

                if (existingPermissionClaims.Any())
                {
                    applicationDbContext.UserClaims.RemoveRange(existingPermissionClaims);
                    await applicationDbContext.SaveChangesAsync(cancellationToken);
                }

                return Result.Success();
            }

            // 3️⃣ If Permissions are provided, assign them as UserClaims
            if (request.Permissions != null && request.Permissions.Any())
            {
                // Validate permissions
                var allowedPermissions = Permissions.GetAllPermissions();
                var invalidPermissions = request.Permissions.Except(allowedPermissions).ToList();

                if (invalidPermissions.Any())
                    return Result.Fail(RoleErrors.InvalidPermissions);

                // Remove existing permission claims
                var existingPermissionClaims = await applicationDbContext.UserClaims
                    .Where(x => x.UserId == userId && x.ClaimType == Permissions.Type)
                    .ToListAsync(cancellationToken);

                if (existingPermissionClaims.Any())
                {
                    applicationDbContext.UserClaims.RemoveRange(existingPermissionClaims);
                }

                // Add new permission claims
                var newClaims = request.Permissions.Select(permission => new IdentityUserClaim<string>
                {
                    UserId = userId,
                    ClaimType = Permissions.Type,
                    ClaimValue = permission
                });

                await applicationDbContext.UserClaims.AddRangeAsync(newClaims, cancellationToken);
                await applicationDbContext.SaveChangesAsync(cancellationToken);

                // Remove user from all roles when assigning direct permissions
                var currentRoles = await userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                return Result.Success();
            }

            // This shouldn't happen if validator is working correctly, but just in case
            return Result.Fail(new Error(
                "AssignRole.InvalidRequest",
                "Either RoleName or Permissions must be provided",
                StatusCodes.Status400BadRequest
            ));
        }
    }
}