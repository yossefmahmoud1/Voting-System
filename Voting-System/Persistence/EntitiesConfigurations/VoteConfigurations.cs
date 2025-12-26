using VotingSystem.Entities.Votes;

namespace VotingSystem.Persistence.EntitiesConfigurations
{
    public class VoteConfigurations : IEntityTypeConfiguration<Vote>
    {
        void IEntityTypeConfiguration<Vote>.Configure(EntityTypeBuilder<Vote> builder)
        {
            builder.HasIndex(x => new { x.PollId, x.UserId }).IsUnique();
        }
    }
}
