using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Domain.Entities
{
    public class Migration
    {
        public string MigrationId { get; set; } = string.Empty;
        public string SourceFileName { get; set; }
        public string UserId { get; set; } = string.Empty;
        public LLMProviderType LLMModel { get; set; } = LLMProviderType.Gemini;
        public TargetLanguage TargetLanguage { get; set; } = TargetLanguage.CSharp;
        public string LastMigrationExecution { get; set; }
        public MigrationStatus Status { get; set; } = MigrationStatus.Completed;
    }
}
