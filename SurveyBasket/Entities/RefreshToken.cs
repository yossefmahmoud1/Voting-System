namespace SurveyBasket.Entities
{
    [Owned]
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expireson { get; set; }
        public DateTime Createdon { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedOn { get; set; } = null;

        public bool Isexpired => DateTime.UtcNow >= Expireson;

        public bool Isactive => RevokedOn == null && !Isexpired; 

    }
}
