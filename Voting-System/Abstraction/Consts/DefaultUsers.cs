namespace VotingSystem.Abstraction.Consts
{
    public static class DefaultUsers
    {
        public const string AdminId = "58a9eb9b-f8b9-4267-a7d7-006b016d6ba0";
        public const string AdminUserName = "admin1";
        public const string AdminPassword = "P@ssword1235Efsa";
        public const string AdminEmail = "admin@surveySystem.com";
        public const string AdminSecurityStamp = "7d97f47978b740f822f2ac6d1f3aac6";
        public const string AdminConcurencyStamp = "fffe5055-7d79-4ad7-a07f-74a10069aea9";
        // Pre-computed password hash for "P@ssword1235Efsa" to avoid dynamic values in HasData
        public const string AdminPasswordHash = "AQAAAAIAAYagAAAAEHHle97gKTePm1/xXB7+FecjiCvo9JyrCl7biGp/YBzNhWlHD4JZKzoWhBODgusZDg==";
        // Pre-computed normalized values to avoid dynamic ToUpper() calls
        public const string AdminUserNameNormalized = "ADMIN1";
        public const string AdminEmailNormalized = "ADMIN@SURVEYSYSTEM.COM";
    }
}
