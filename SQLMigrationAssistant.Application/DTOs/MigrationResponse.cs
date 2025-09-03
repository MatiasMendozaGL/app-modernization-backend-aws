using SQLMigrationAssistant.Domain.Enums;
using System.Text.Json.Serialization;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class MigrationResponse
    {
        public string MigrationId { get; set; }
        public string SourceFileName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LLMProviderType LLMModel { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TargetLanguage TargetLanguage { get; set; }
        public string LastMigrationExecution { get; set; }
        public IEnumerable<string> GeneratedCodeFiles { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MigrationStatus Status { get; set; }
    }
}
