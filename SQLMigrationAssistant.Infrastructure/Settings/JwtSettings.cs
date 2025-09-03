namespace SQLMigrationAssistant.Infrastructure.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "SQLMigrationAssistant";
        public string Audience { get; set; } = "SQLMigrationAssistant";
        public int AccessTokenExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}
