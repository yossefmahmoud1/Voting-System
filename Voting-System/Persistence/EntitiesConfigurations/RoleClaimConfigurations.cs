using Microsoft.AspNetCore.Identity;
using VotingSystem.Abstraction.Consts;
using VotingSystem.Entities.Answers;

namespace VotingSystem.Persistence.EntitiesConfigurations
{
    public class RoleClaimConfigurations : IEntityTypeConfiguration<IdentityRoleClaim<string>>
    {


        public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
        {
            // Use static list instead of dynamic calculation to prevent model changes on every build
            var adminClaims = new List<IdentityRoleClaim<string>>
            {
                new() { Id = 1, ClaimType = Permissions.Type, ClaimValue = Permissions.GetPolls, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 2, ClaimType = Permissions.Type, ClaimValue = Permissions.AddPolls, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 3, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdatePolls, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 4, ClaimType = Permissions.Type, ClaimValue = Permissions.DeletePolls, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 5, ClaimType = Permissions.Type, ClaimValue = Permissions.GetQuestions, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 6, ClaimType = Permissions.Type, ClaimValue = Permissions.AddQuestions, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 7, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateQuestions, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 8, ClaimType = Permissions.Type, ClaimValue = Permissions.GetUsers, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 9, ClaimType = Permissions.Type, ClaimValue = Permissions.AddUsers, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 10, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateUsers, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 11, ClaimType = Permissions.Type, ClaimValue = Permissions.GetRoles, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 12, ClaimType = Permissions.Type, ClaimValue = Permissions.AddRoles, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 13, ClaimType = Permissions.Type, ClaimValue = Permissions.UpdateRoles, RoleId = DefaultRoles.AdminRoleId },
                new() { Id = 14, ClaimType = Permissions.Type, ClaimValue = Permissions.Results, RoleId = DefaultRoles.AdminRoleId }
            };
            
            builder.HasData(adminClaims);
        }
    }
}