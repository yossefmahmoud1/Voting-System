using SurveyBasket.Entities.Votes;

namespace SurveyBasket.Persistence.EntitiesConfigurations
{
    namespace SurveyBasket.Persistence.EntitiesConfigurations
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
