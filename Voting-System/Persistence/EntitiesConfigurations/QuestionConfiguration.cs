using VotingSystem.Entities.Answers;
using VotingSystem.Entities.Questions;

namespace VotingSystem.Persistence.EntitiesConfigurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {


      

        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasIndex(x => new { x.pollId, x.Content }).IsUnique();
            builder.Property(x => x.Content).HasMaxLength(1000);



        }
    }
}
