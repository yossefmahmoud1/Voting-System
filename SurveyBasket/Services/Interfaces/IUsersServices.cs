using SurveyBasket.Dtos.Common;
using SurveyBasket.Dtos.Users;

namespace SurveyBasket.Services.Interfaces
{
    public interface IUsersServices
    {
        Task<PaginatedList<UserResponse>> GetAllUsersAsync(RequestFilters? filters = null, CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> GetUserDetails(string id, CancellationToken cancellationToken = default);
         Task<Result<UserResponse>> AddUserAsync(AddUserRequest request, CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> UpdateUserAsync(
             string id,
             UpdateUsersRequest request,
             CancellationToken cancellationToken = default);

        Task<Result> Unlock(string id);
        Task<Result> ToggleStatus(string id);
        }
}
