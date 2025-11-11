
using SurveyBasket.Abstraction;

namespace SurveyBasket.Errors
{
    public static class UserErrors
    {

        public static readonly Error InvalidCredentials = 
            new Error("User.InvalidCredentials", "Invalid Email or Password");
    };
}
