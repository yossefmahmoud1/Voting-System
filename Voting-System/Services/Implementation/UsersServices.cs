using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Dtos.Common;
using VotingSystem.Dtos.Users;
using VotingSystem.Extensions;
using VotingSystem.Repositeryes.Implementation;
using VotingSystem.Repositeryes.Interfaces;
using VotingSystem.Services.Interfaces;

namespace VotingSystem.Services.Implementation
{
    public class UsersServices(UserManager<Application_User> userManager,
        ApplicationDbContext dbContext,
        IUserRepository userRepository,
        IRoleService roleService) : IUsersServices
    {
        private readonly UserManager<Application_User> userManager = userManager;
        private readonly ApplicationDbContext dbContext = dbContext;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRoleService roleService = roleService;

        public async Task<PaginatedList<UserResponse>> GetAllUsersAsync(
     RequestFilters? filters = null,
     CancellationToken cancellationToken = default)
        {
            filters ??= new RequestFilters();

            // Get user IDs that don't have Member role
            var memberRoleId = await dbContext.Roles
                .Where(r => r.Name == DefaultRoles.Member)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var userIdsWithMemberRole = memberRoleId != null
                ? await dbContext.UserRoles
                    .Where(ur => ur.RoleId == memberRoleId)
                    .Select(ur => ur.UserId)
                    .ToListAsync(cancellationToken)
                : new List<string>();

            // Get users excluding those with Member role
            var usersQuery = dbContext.Users
                .Where(u => !userIdsWithMemberRole.Contains(u.Id));

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(filters.SearchValue))
            {
                var searchValue = filters.SearchValue.ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(searchValue)) ||
                    (u.FristName != null && u.FristName.ToLower().Contains(searchValue)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchValue)));
            }

            // Apply sorting
            usersQuery = filters.SortColumn?.ToLower() switch
            {
                "email" => filters.SortDirection?.ToLower() == "desc"
                    ? usersQuery.OrderByDescending(u => u.Email)
                    : usersQuery.OrderBy(u => u.Email),
                "firstname" => filters.SortDirection?.ToLower() == "desc"
                    ? usersQuery.OrderByDescending(u => u.FristName)
                    : usersQuery.OrderBy(u => u.FristName),
                "lastname" => filters.SortDirection?.ToLower() == "desc"
                    ? usersQuery.OrderByDescending(u => u.LastName)
                    : usersQuery.OrderBy(u => u.LastName),
                _ => usersQuery.OrderBy(u => u.Email)
            };

            // Get total count before pagination
            var totalCount = await usersQuery.CountAsync(cancellationToken);

            // Apply pagination and get user data
            var users = await usersQuery
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Select(u => new
                {
                    u.Id,
                    u.FristName,
                    u.LastName,
                    u.Email,
                    u.IsDisabled
                })
                .ToListAsync(cancellationToken);

            // Get roles for each user
            var userIds = users.Select(u => u.Id).ToList();
            var userRolesDict = userIds.Any()
                ? await dbContext.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId))
                    .Join(dbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, RoleName = r.Name })
                    .GroupBy(x => x.UserId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(x => x.RoleName!).ToList(),
                        cancellationToken)
                : new Dictionary<string, List<string>>();

            // Map to UserResponse and create PaginatedList
            var userResponses = users.Select(u => new UserResponse(
                u.Id,
                u.Email ?? string.Empty,
                u.FristName ?? string.Empty,
                u.LastName ?? string.Empty,
                u.IsDisabled,
                userRolesDict.GetValueOrDefault(u.Id, new List<string>())
            )).ToList();

            // Create PaginatedList using reflection or helper method
            // Since constructor is private, we'll create it via CreateAsync with materialized data
            var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);
            return new PaginatedList<UserResponse>
            {
                Items = userResponses,
                PageNumber = filters.PageNumber,
                TotalPages = totalPages
            };
        }






        public async Task<Result<UserResponse>> GetUserDetails(string id, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Fail<UserResponse>(UserErrors.UserNotFound);
            }
            var roles = await userManager.GetRolesAsync(user);
            var response = (user, roles).Adapt<UserResponse>();
            return Result.Success(response);


        }

        public async Task<Result<UserResponse>> AddUserAsync(AddUserRequest request, CancellationToken cancellationToken = default)
        {
        
            var emailExists = await userManager.FindByEmailAsync(request.Email);
            if (emailExists != null)
            {
                return Result.Fail<UserResponse>(UserErrors.EmailAlreadyExists);
            }
        // 2️ Check username uniqueness
    var userNameExists = await userManager.FindByNameAsync(request.UserName);
            if (userNameExists != null)
            {
                var suggestions = await GenerateUsernameSuggestionsAsync(
                    request.UserName,
                    cancellationToken);

                return Result.Fail<UserResponse>(
                    UserErrors.UsernameTaken(suggestions)
                );
            }
            var allowedRoles = (await roleService.GetAllAsync(
           cancellationToken: cancellationToken))
       .Select(r => r.Name)
       .ToList();

            var invalidRoles = request.Roles
                .Except(allowedRoles, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (invalidRoles.Any())
            {
                return Result.Fail<UserResponse>(RoleErrors.InvalidRoles);
            }

            var user =request.Adapt<Application_User>();
            user.EmailConfirmed=true;
            user.Email=request.Email;
            var result = await userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, request.Roles);
                var response =(user,request.Roles).Adapt<UserResponse>();
                return Result.Success(response);

            }
            var error = result.Errors.First();
            return Result.Fail<UserResponse>(new Error(
                "User.RegisterFailed",
                error.Description,
                400
            ));
        }
        public async Task<Result<UserResponse>> UpdateUserAsync(
            string id,
            UpdateUsersRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1️⃣ Check user existence
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
                return Result.Fail<UserResponse>(UserErrors.UserNotFound);

            // 2️⃣ Check email uniqueness
            var emailIsExists = await userManager.Users
                .AnyAsync(x => x.Email == request.Email && x.Id != id, cancellationToken);

            if (emailIsExists)
                return Result.Fail<UserResponse>(UserErrors.EmailAlreadyExists);

            // 3️⃣ Validate roles
            var allowedRoles = (await roleService.GetAllAsync(
                    cancellationToken: cancellationToken))
                .Select(x => x.Name)
                .ToList();

            var invalidRoles = request.Roles
                .Except(allowedRoles, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (invalidRoles.Any())
                return Result.Fail<UserResponse>(RoleErrors.InvalidRoles);

            // 4️⃣ Update user fields (Safe mapping)
            user.Email = request.Email;
            user.FristName = request.FristName;
            user.LastName = request.LastName;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                return Result.Fail<UserResponse>(
                    new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
                );
            }

            // 5️⃣ Update roles safely via Identity
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRolesAsync(user, request.Roles);

            // 6️⃣ Return response
            var response = (user, request.Roles).Adapt<UserResponse>();
            return Result.Success(response);
        }

        public async Task<Result> ToggleStatus(string id)
        {
            if (await userManager.FindByIdAsync(id) is not { } user)
                return Result.Fail(UserErrors.UserNotFound);

            user.IsDisabled = !user.IsDisabled;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();

            return Result.Fail(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> Unlock(string id)
        {
            if (await userManager.FindByIdAsync(id) is not { } user)
                return Result.Fail(UserErrors.UserNotFound);

            var result = await userManager.SetLockoutEndDateAsync(user, null);

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();

            return Result.Fail(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }





        //======================================//

        private async Task<List<string>> GenerateUsernameSuggestionsAsync(string baseUsername, CancellationToken cancellationToken = default)
        {
            var suggestions = new List<string>();
            var random = new Random();

            // Generate suggestions by appending numbers
            for (int i = 1; i <= 5; i++)
            {
                var suggestion = $"{baseUsername}{random.Next(100, 999)}";
                if (!await _userRepository.ExistsByUserNameAsync(suggestion, cancellationToken))
                {
                    suggestions.Add(suggestion);
                    if (suggestions.Count >= 3) break;
                }
            }

            // If we don't have enough suggestions, try with different patterns
            if (suggestions.Count < 3)
            {
                for (int i = 1; i <= 5 && suggestions.Count < 3; i++)
                {
                    var suggestion = $"{baseUsername}_{random.Next(1000, 9999)}";
                    if (!await _userRepository.ExistsByUserNameAsync(suggestion, cancellationToken))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            // If still not enough, add timestamp-based suggestions
            if (suggestions.Count < 3)
            {
                var timestamp = DateTime.UtcNow.Ticks % 10000;
                var suggestion = $"{baseUsername}{timestamp}";
                if (!await _userRepository.ExistsByUserNameAsync(suggestion, cancellationToken))
                {
                    suggestions.Add(suggestion);
                }
            }

            return suggestions;
        }


    }

}