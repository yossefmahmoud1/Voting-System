using Microsoft.AspNetCore.Identity;
using VotingSystem.Abstraction.Consts;

namespace VotingSystem.Persistence.EntitiesConfigurations
{
    public class UserConfiguration: IEntityTypeConfiguration<Application_User>
    {
       
            public void Configure(EntityTypeBuilder<Application_User> builder)
            {

                builder.Property(x => x.FristName).HasMaxLength(100);
                builder.Property(x => x.LastName).HasMaxLength(100);

            // Use pre-computed constants instead of dynamic calculations
            // This prevents EF Core from detecting model changes on every build
            builder.HasData(new Application_User
            {
                Id=DefaultUsers.AdminId,
                UserName=DefaultUsers.AdminUserName,
                NormalizedUserName=DefaultUsers.AdminUserNameNormalized,
                Email=DefaultUsers.AdminEmail,
                NormalizedEmail=DefaultUsers.AdminEmailNormalized,
                SecurityStamp=DefaultUsers.AdminSecurityStamp,
                ConcurrencyStamp=DefaultUsers.AdminConcurencyStamp,
                EmailConfirmed=true,
                PasswordHash=DefaultUsers.AdminPasswordHash,
            });
            }
        }
    }

