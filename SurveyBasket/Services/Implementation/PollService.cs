using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using SurveyBasket.Dtos;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class PollService : IPollService
    {
        private static readonly List<Poll> _polls = new List<Poll>();

        public Poll Add(Poll poll)
        {
            poll.Id = _polls.Count + 1;
            _polls.Add(poll);

            return poll;
        }

        public IEnumerable<PollResponse> GetAll()
        {
            var response = _polls.Select(p => p.Adapt<PollResponse>()).ToList();
            return response;
        }

        public Poll? GetById(int Id)
        {
            var poll = _polls.SingleOrDefault(x => x.Id == Id);

            return poll;
        }

        public bool Update(int id, Poll poll)
        {
            var targetPoll = _polls.SingleOrDefault(x => x.Id == id);

            if (targetPoll == null)
                return false;

            poll.Adapt(targetPoll);

            return true;
        }

        public bool Delete(int id)
        {
            var targetPoll = _polls.SingleOrDefault(x => x.Id == id);

            if (targetPoll == null)
                return false;

            _polls.Remove(targetPoll);
            return true;
        }
    }
}
