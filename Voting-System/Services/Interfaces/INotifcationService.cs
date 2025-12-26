namespace VotingSystem.Services.Implementation
{
    public interface INotifcationService
    {

        Task SendNewPollNotficationsAsync(int? pollId);
    }
}
