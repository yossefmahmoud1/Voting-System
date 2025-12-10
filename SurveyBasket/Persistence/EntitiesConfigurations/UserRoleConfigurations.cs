using Microsoft.AspNetCore.Identity;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Entities.Questions;

namespace SurveyBasket.Persistence.EntitiesConfigurations
{
    public class UserRoleConfigurations : IEntityTypeConfiguration<IdentityUserRole<string>>
    {




     

        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(new IdentityUserRole<string>
                {
              UserId=DefaultUsers.AdminId,
                RoleId=DefaultRoles.AdminRoleId
            }
               
            );
        }
    }
}

