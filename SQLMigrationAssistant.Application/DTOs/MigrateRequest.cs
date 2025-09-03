using MediatR;
using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class MigrateRequest : IRequest<MigrationResponse>
    {
        public Stream FileContent { get; set; } = new MemoryStream();
        public string FileName { get; set; } = string.Empty;
        public string FileContentType { get; set; } = string.Empty;
        public LLMProviderType LlmProvideType { get; set; } = LLMProviderType.Gemini;
        public TargetLanguage TargetLanguage { get; set; } = TargetLanguage.CSharp;
        public string Prompt { get; set; } = string.Empty;
        public string UserId { get; set; }
    }
}
