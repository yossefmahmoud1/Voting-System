using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Entities;
using VotingSystem.Persistence;
using VotingSystem.Repositeryes.Interfaces;

namespace VotingSystem.Repositeryes.Implementation;

public class UserRepository : IUserRepository
{
    private readonly UserManager<Application_User> _userManager;
    private readonly ApplicationDbContext _context;

    public UserRepository(UserManager<Application_User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Application_User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(id);

    }

    public async Task<Application_User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<Application_User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<Application_User?> GetByIdWithRefreshTokensAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .Include(u => u.refreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Application_User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .Include(u => u.refreshTokens)
            .FirstOrDefaultAsync(u => u.refreshTokens.Any(t => t.Token == refreshToken), cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<Application_User> CreateAsync(Application_User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        
        return user;
    }

    public async Task<bool> CheckPasswordAsync(Application_User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task UpdateAsync(Application_User user, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task<IList<string>> GetRolesAsync(Application_User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
    public async Task<string> GenerateEmailConfirmationTokenAsync(Application_User user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }
    public async Task<bool> ConfirmEmailAsync(Application_User user, string token)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }
    public async Task<string> GenratePasswordResetTokenAsync(Application_User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(
     Application_User user,
     string token,
     string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }
    public async Task AddToRoleAsync(Application_User user, string roleName)
    {
        await _userManager.AddToRoleAsync(user, roleName);
    }


}

