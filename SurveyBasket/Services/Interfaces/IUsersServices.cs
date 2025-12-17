using SurveyBasket.Dtos.Users;

namespace SurveyBasket.Services.Interfaces
{
    public interface IUsersServices
    {
        Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> GetUserDetails(string id, CancellationToken cancellationToken = default);
    }
}
