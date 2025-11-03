using SurveyBasket.Dtos;

namespace SurveyBasket.Services.Interfaces
{
    public interface IPollService
    {
      IEnumerable<PollResponse> GetAll();
        Poll?  GetById(int Id);
        Poll  Add (Poll poll);
        bool Update(int id,Poll poll);
        bool Delete(int id);
    }
}
