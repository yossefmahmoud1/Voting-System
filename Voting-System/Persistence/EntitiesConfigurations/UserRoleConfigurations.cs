using Microsoft.AspNetCore.Identity;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Entities.Questions;

namespace VotingSystem.Persistence.EntitiesConfigurations
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

