using SurveyBasket.Entities;

namespace SurveyBasket.Repositeryes.Interfaces;

public interface IUserRepository
{
    Task<Application_User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Application_User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Application_User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<Application_User?> GetByIdWithRefreshTokensAsync(string id, CancellationToken cancellationToken = default);
    Task<Application_User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<Application_User> CreateAsync(Application_User user, string password, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(Application_User user, string password);
    Task UpdateAsync(Application_User user, CancellationToken cancellationToken = default);
    Task<IList<string>> GetRolesAsync(Application_User user);
}

