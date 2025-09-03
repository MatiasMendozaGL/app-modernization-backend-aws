namespace SQLMigrationAssistant.Infrastructure.Settings
{
    public class RetrySettings
    {
        public const string SectionName = "RetrySettings";

        public int MaxAttempts { get; set; } = 3;
        public int InitialBackoffSeconds { get; set; } = 2;
        public int MaxBackoffSeconds { get; set; } = 60;
        public double BackoffMultiplier { get; set; } = 2.0;
        public string[] RetryableStatusCodes { get; set; } = new[] { "Unavailable", "DeadlineExceeded", "ResourceExhausted" };
    }
}