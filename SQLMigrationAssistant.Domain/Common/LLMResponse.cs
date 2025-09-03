namespace SQLMigrationAssistant.Domain.Common
{
    public class LLMResponse
    {
        public bool IsSuccess { get; set; }
        public string Output { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
