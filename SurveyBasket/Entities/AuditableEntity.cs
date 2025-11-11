namespace SurveyBasket.Entities
{
    public class AuditableEntity
    {
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedById { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public Application_User CreatedBy { get; set; } = null!;
        public Application_User? UpdatedBy { get; set; } = null!;
    }
}
