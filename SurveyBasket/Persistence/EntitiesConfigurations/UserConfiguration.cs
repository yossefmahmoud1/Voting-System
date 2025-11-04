namespace SurveyBasket.Persistence.EntitiesConfigurations
{
    public class UserConfiguration: IEntityTypeConfiguration<Application_User>
    {
       
            public void Configure(EntityTypeBuilder<Application_User> builder)
            {

                builder.Property(x => x.FristName).HasMaxLength(100);
                builder.Property(x => x.LastName).HasMaxLength(100);
            }
        }
    }

