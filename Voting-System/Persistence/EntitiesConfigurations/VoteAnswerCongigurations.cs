using VotingSystem.Entities.Votes;

namespace VotingSystem.Persistence.EntitiesConfigurations
{
    namespace VotingSystem.Persistence.EntitiesConfigurations
    {
        public class VoteConfigurations : IEntityTypeConfiguration<VoteAnswer>
        {
            public void Configure(EntityTypeBuilder<VoteAnswer> builder)
            {
                builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();
            }


        }
    }
}
