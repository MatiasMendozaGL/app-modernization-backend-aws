namespace SQLMigrationAssistant.Infrastructure.Settings
{
    public class CloudStorageSettings
    {
        public const string SectionName = "CloudStorage";
        public string ProjectId { get; set; }
        public string CredentialsPath { get; set; }
        public string BucketName { get; set; } = string.Empty;
        public int MaxFileSizeMB { get; set; } = 50;
        public string AllowedFileTypes { get; set; } = ".sql,.txt";
    }
}
