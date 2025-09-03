namespace SQLMigrationAssistant.Infrastructure.Settings
{
    public class VertexAiSettings
    {
        public const string SectionName = "VertexAI";
        public required string ProjectId { get; set; }
        public required string Location { get; set; }
        public required string ModelName { get; set; }
        public required string Endpoint { get; set; }
        public required int MaxOutputTokens { get; set; }
        public required float Temperature { get; set; }
        public required float TopP { get; set; }
        public required string Prompt { get; set; }
    }
}
