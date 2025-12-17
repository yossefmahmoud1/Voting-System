using SurveyBasket.Dtos.Questions;
using SurveyBasket.Dtos.Users;
using SurveyBasket.Dtos.Votes;
using SurveyBasket.Entities.Answers;
using SurveyBasket.Entities.Questions;
using SurveyBasket.Entities.Votes;

namespace SurveyBasket.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<QuestionRequest, Question>()
            .Map(dest => dest.Answers, src => src.Answers.Select(answer => new Answer { Content = answer }));

        config.NewConfig<VoteAnswerRequest, VoteAnswer>()
            .Map(dest => dest.QuestionId, src => src.QuestionId)
            .Map(dest => dest.AnswerId, src => src.answerId);
        config.NewConfig<(Application_User user, IList<string> roles),UserResponse> ()
            .Map(dest => dest, src => src.user)
            .Map(dest => dest.Roles, src => src.roles);


    }
}