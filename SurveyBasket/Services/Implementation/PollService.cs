using Microsoft.AspNetCore.Http.HttpResults;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class PollService : IPollService
    {
        private static readonly List<Poll> _polls = new List<Poll>
        {
         
        };

        public Poll Add(Poll poll)
        {
            poll.Id = _polls.Count + 1;
            _polls.Add(poll);
            return poll;
        }

        public IEnumerable<Poll> GetAll()
        {
            return _polls;
        }

        public Poll? GetById(int Id)
        {
            var poll = _polls.SingleOrDefault(x => x.Id == Id);
           return poll;
        }

        public bool Update(int id, Poll poll)
        {
            var targetPoll = GetById( id);

            if (targetPoll == null)
                return false;
            targetPoll.Title = poll.Title;
            targetPoll.Description = poll.Description;

            return true;
        }
        public bool Delete(int id)
        {
            var targetPoll = GetById(id);
            if(targetPoll == null)
                return false;
         _polls.Remove(targetPoll);

            return true;
        }

     
    }
}
