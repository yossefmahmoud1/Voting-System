using VotingSystem.Dtos.Questions;
using VotingSystem.Dtos.Users;
using VotingSystem.Dtos.Votes;
using VotingSystem.Entities.Answers;
using VotingSystem.Entities.Questions;
using VotingSystem.Entities.Votes;

namespace VotingSystem.Mapping;

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