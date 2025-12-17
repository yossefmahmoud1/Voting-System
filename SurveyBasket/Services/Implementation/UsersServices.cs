using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Dtos.Users;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class UsersServices(UserManager<Application_User> userManager, ApplicationDbContext dbContext):IUsersServices
    {
        private readonly UserManager<Application_User> userManager = userManager;
        private readonly ApplicationDbContext dbContext = dbContext;

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken =default)
        {
            return await (
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
            }
        )
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
        .ToListAsync(cancellationToken);
        }
        public async Task<Result<UserResponse>> GetUserDetails(string id,CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Fail<UserResponse>(UserErrors.UserNotFound);
            }
            var roles = await userManager.GetRolesAsync(user);
           var response =(user , roles).Adapt<UserResponse>();
            return Result.Success(response);


        }

    }

}

