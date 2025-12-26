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

            var baseQuery =
                from u in dbContext.Users
                join ur in dbContext.UserRoles
                    on u.Id equals ur.UserId
                join r in dbContext.Roles
                    on ur.RoleId equals r.Id
                    into roles
                where !roles.Any(x => x.Name == DefaultRoles.Member)
                select new
                {
                    u.Id,
                    u.FristName,
                    u.LastName,
                    u.Email,
                    u.IsDisabled,
                    Roles = roles.Select(x => x.Name).ToList()
                };

            var query = baseQuery
                .GroupBy(x => new
                {
                    x.Id,
                    x.FristName,
                    x.LastName,
                    x.Email,
                    x.IsDisabled
                })
                .Select(g => new UserResponse(
                    g.Key.Id,
                    g.Key.Email!,
                    g.Key.FristName,
                    g.Key.LastName,
                    g.Key.IsDisabled,
                    g.SelectMany(x => x.Roles).ToList()
                ))
                .ApplyFilters(
                    filters,
                    u => u.Email,
                    u => u.FristName,
                    u => u.LastName);

            // Default sort if client didn't specify any
            if (string.IsNullOrWhiteSpace(filters.SortColumn))
                query = query.OrderBy(u => u.Email);

            return await PaginatedList<UserResponse>.CreateAsync(
                query,
                filters.PageNumber,
                filters.PageSize,
                cancellationToken);
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