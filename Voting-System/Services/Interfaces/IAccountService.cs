using VotingSystem.Dtos.User;

namespace VotingSystem.Services.Interfaces
{
    public interface IAccountService
    {
        public  Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId, CancellationToken cancellationToken =default);
        public  Task<Result<UserProfileResponse>> UpdateUserProfileAsync(string userId, UpdateUserRequest updateUserRequest, CancellationToken cancellationToken =default);
        public Task<Result> ChangePasswordAsync(string UserId, ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken = default);



    }
}
