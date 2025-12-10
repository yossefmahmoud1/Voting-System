using Microsoft.AspNetCore.Identity;
using SurveyBasket.Dtos.User;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class UserService(UserManager<Application_User> userManager):IUserService
    {
        private readonly UserManager<Application_User> userManager = userManager;

        public async Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId ,CancellationToken cancellationToken)
        {

           var User= await userManager.Users
                .Where(x => x.Id == userId)
                .ProjectToType<UserProfileResponse>()
                .SingleOrDefaultAsync(cancellationToken);



            if (User==null)
              {
                return Result.Fail<UserProfileResponse>(UserErrors.UserNotFound);
            }
 

            return Result.Success(User);

        }
        public async Task<Result<UserProfileResponse>> UpdateUserProfileAsync(
         string userId,
         UpdateUserRequest updateUserRequest,
         CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user == null)
                return Result.Fail<UserProfileResponse>(UserErrors.UserNotFound);

            updateUserRequest.Adapt(user);

            await userManager.UpdateAsync(user);

            var response = user.Adapt<UserProfileResponse>();
            return Result.Success(response);
        }

        public async Task<Result> ChangePasswordAsync(string UserId , ChangePasswordRequest changePasswordRequest ,CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                .SingleOrDefaultAsync(x => x.Id == UserId, cancellationToken);
          var result=  await userManager.ChangePasswordAsync(user!, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);
            if (result.Succeeded)
                return Result.Success();
            var error = result.Errors.First();
            return Result.Fail(new Error(error.Code , error.Description , StatusCodes.Status404NotFound));
        }
    }

}


    

