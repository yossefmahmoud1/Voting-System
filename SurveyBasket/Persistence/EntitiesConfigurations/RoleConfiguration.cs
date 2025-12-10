using Microsoft.AspNetCore.Identity;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Entities.Questions;

namespace SurveyBasket.Persistence.EntitiesConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {




        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.HasData([

                new ApplicationRole
            {
                Id = DefaultRoles.AdminRoleId,
                Name = DefaultRoles.Admin,
                NormalizedName = DefaultRoles.Admin.ToUpper(),
                ConcurrencyStamp = DefaultRoles.AdminRoleConcurencyStamp,
            },
                new ApplicationRole
            {
                Id = DefaultRoles.MemberRoleId,
                Name = DefaultRoles.Member,
                NormalizedName = DefaultRoles.Member.ToUpper(),
                ConcurrencyStamp = DefaultRoles.MemberRoleConcurencyStamp,
                IsDefault=true,
            },
                ]);

        }
    }
}


