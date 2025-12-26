namespace VotingSystem.Entities.Votes
{
    public class Vote
    {
        public int id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; } = default!;
        public string UserId { get; set; } = string.Empty;

        public Application_User User { get; set; } = default!;
        public DateTime SubmitedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VoteAnswer> VoteAnswers { get; set; } = [];
    }
}
