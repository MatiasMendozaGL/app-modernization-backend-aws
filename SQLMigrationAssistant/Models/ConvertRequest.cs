using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.API.Models
{
    public class ConvertRequest
    {
        public IFormFile? File { get; set; }
        public LLMProviderType LLMProvider { get; set; } = LLMProviderType.Gemini;
        public TargetLanguage TargetLanguage { get; set; } = TargetLanguage.CSharp;
    }
}
