using Microsoft.AspNetCore.Identity;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Entities.Questions;

namespace VotingSystem.Persistence.EntitiesConfigurations
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


