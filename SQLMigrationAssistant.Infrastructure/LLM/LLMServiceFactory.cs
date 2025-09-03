using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Infrastructure.LLM
{
    public class LLMServiceFactory : ILLMServiceFactory
    {
        private readonly IEnumerable<ILLMService> _llmServices;

        // Injects all implementations of ILLMService
        public LLMServiceFactory(IEnumerable<ILLMService> llmServices)
        {
            _llmServices = llmServices;
        }

        public ILLMService GetLLMService(LLMProviderType providerType)
        {
            // Searchs in the collection the service that belongs to the requested provider type
            var service = _llmServices.FirstOrDefault(s => s.ProviderType == providerType);

            if (service == null)
            {
                throw new InvalidOperationException($"Service for provider {providerType} not found.");
            }

            return service;
        }
    }
}
