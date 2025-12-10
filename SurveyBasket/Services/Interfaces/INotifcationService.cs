namespace SurveyBasket.Services.Implementation
{
    public interface INotifcationService
    {

        Task SendNewPollNotficationsAsync(int? pollId);
    }
}
