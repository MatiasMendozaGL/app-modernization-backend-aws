using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain.Common;
using SQLMigrationAssistant.Domain.Enums;
using System.Reflection;

namespace SQLMigrationAssistant.Infrastructure.LLM
{
    public class OpenAIService : ILLMService
    {
        private readonly ILogger<OpenAIService> _logger;

        public LLMProviderType ProviderType => LLMProviderType.OpenAI;

        public OpenAIService(ILogger<OpenAIService> logger)
        {
            _logger = logger;
        }

        public async Task<LLMResponse> ConvertAsync(string sqlContent, string targetLanguage, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending SQL content to OpenAI for conversion to {TargetLanguage}", targetLanguage);

            // TODO: Replace this temporal logic with OpenAI implementation
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SQLMigrationAssistant.Infrastructure.output_example.txt";
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var output = await reader.ReadToEndAsync();

            return new LLMResponse { IsSuccess = true, Output = output };
        }
    }
}
