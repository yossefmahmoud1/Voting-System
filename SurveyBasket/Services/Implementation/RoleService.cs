using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Dtos.Roles;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext applicationDbContext) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> roleManager = roleManager;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;

        public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? IncludeDiasbeled = false, CancellationToken cancellationToken = default)
        {
            var Roles = await roleManager.Roles.Where(x =>
        !x.IsDefault &&
        (IncludeDiasbeled == true || !x.IsDeleted)
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
    }
}