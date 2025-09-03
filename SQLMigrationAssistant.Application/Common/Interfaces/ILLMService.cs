using SQLMigrationAssistant.Domain.Common;
using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface ILLMService
    {
        LLMProviderType ProviderType { get; }
        Task<LLMResponse> ConvertAsync(string sqlContent, string targetLanguage, CancellationToken cancellationToken = default);
    }
}
