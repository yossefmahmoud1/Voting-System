using SurveyBasket.Entities.Answers;
using SurveyBasket.Entities.Questions;

namespace SurveyBasket.Persistence.EntitiesConfigurations
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
