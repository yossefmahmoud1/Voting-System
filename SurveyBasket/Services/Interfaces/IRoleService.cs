using SurveyBasket.Dtos.Roles;

namespace SurveyBasket.Services.Interfaces
{
    public interface IRoleService
    {
          Task<IEnumerable<RoleResponse>> GetAllAsync(bool IncludeDiasbeled = false, CancellationToken cancellationToken=default);
        Task<Result<RoleDetailsResponse>> GetAsync(string Id, CancellationToken cancellationToken = default);
        Task<Result<RoleDetailsResponse>> AddAsync(RoleRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(string id, RoleRequest request);
        Task<Result> ToggleStatusAsync(string id);
    }
}
