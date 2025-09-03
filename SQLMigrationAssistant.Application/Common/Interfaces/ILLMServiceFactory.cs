using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface ILLMServiceFactory
    {
        ILLMService GetLLMService(LLMProviderType llmProviderType);
    }
}
