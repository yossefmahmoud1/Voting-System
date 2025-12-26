using VotingSystem.Entities.Answers;

namespace VotingSystem.Entities.Questions
{
    public class Question :AuditableEntity
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int pollId { get; set; }
        public bool IsActive { get; set; } = true;
        public Poll poll { get; set; } = default!;

        public ICollection<Answer> Answers { get; set; } = [];
   
    }
}
