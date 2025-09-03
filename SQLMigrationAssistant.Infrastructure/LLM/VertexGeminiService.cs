using Google.Cloud.AIPlatform.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain.Common;
using SQLMigrationAssistant.Domain.Enums;
using SQLMigrationAssistant.Infrastructure.Settings;

namespace SQLMigrationAssistant.Infrastructure.LLM
{
    public class VertexGeminiService : ILLMService
    {
        private readonly ILogger<VertexGeminiService> _logger;
        private readonly VertexAiSettings _settings;
        private readonly PredictionServiceClient _predictionServiceClient;
        private readonly IPromptProvider _promptProvider;
        private readonly IRetryPolicy _retryPolicy;

        public LLMProviderType ProviderType => LLMProviderType.Gemini;

        public VertexGeminiService(ILogger<VertexGeminiService> logger, IOptions<VertexAiSettings> settings, 
            IPromptProvider promptProvider, IRetryPolicy retryPolicy)
        {
            _logger = logger;
            _settings = settings.Value;
            _promptProvider = promptProvider;
            _retryPolicy = retryPolicy;

            // This client is thread-safe
            _predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{_settings.Endpoint}"
            }.Build();
        }

        public async Task<LLMResponse> ConvertAsync(string sqlContent, string targetLanguage, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending file content to Vertex AI Gemini for conversion to {TargetLanguage}", targetLanguage);

            // Builds the endpoint of the model
            var endpointName = EndpointName.FromProjectLocationPublisherModel(
                _settings.ProjectId,
                _settings.Location,
                "google",
                _settings.ModelName);

            // Builds the prompt
            var prompt = BuildPrompt(sqlContent, targetLanguage);

            // Creates the request to the SDK
            var request = new GenerateContentRequest
            {
                Model = endpointName.ToString(),
                Contents =
                {
                    new Content
                    {
                        Role = "user",
                        Parts = { new Part { Text = prompt } }
                    }
                },
                // Generation parameters
                // Temperature: Controls the randomness of the output; higher values mean more creative and diverse responses.
                // TopP: Defines the nucleus sampling threshold, where the model considers only tokens whose cumulative probability exceeds this value.
                // MaxOutputTokens: Sets the maximum number of tokens(words or parts of words) that the model will generate in its response
                GenerationConfig = new GenerationConfig { MaxOutputTokens = _settings.MaxOutputTokens, Temperature = _settings.Temperature, TopP = _settings.TopP }
            };

            try
            {
                // Sends request to the API
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    // Check if cancellation was requested before making the API call
                    cancellationToken.ThrowIfCancellationRequested();

                    _logger.LogDebug("Attempting to call Vertex AI Gemini API");

                    // Sends request to the API
                    return await _predictionServiceClient.GenerateContentAsync(request, cancellationToken);
                });

                // Extracts the response content
                string generatedCode = response.Candidates.FirstOrDefault()?
                                               .Content.Parts.FirstOrDefault()?
                                               .Text.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(generatedCode))
                {
                    _logger.LogWarning("Vertex AI Gemini returned an empty response.");
                    return new LLMResponse { IsSuccess = false, Output = "The model returned an empty response." };
                }

                _logger.LogInformation("Successfully received code from Vertex AI Gemini.");

                return new LLMResponse { IsSuccess = true, Output = generatedCode };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "An error occurred while calling Vertex AI. Status: {Status}, Details: {Details}", ex.Status.StatusCode, ex.Status.Detail);
                return new LLMResponse { IsSuccess = false, ErrorMessage = $"API Error: {ex.Status.Detail}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in VertexGeminiService.");
                return new LLMResponse { IsSuccess = false, ErrorMessage = "An unexpected error occurred." };
            }
        }

        private string BuildPrompt(string sqlContent, string targetLanguage)
        {
            // 1. Loads the master template
            string promptTemplate = _promptProvider.GetPrompt("CodeGeneration_Template_Prompt.txt");

            // 2. Loads the instructions specific for each targetLanguage
            string platformInstructions;
            if (targetLanguage.Equals(TargetLanguage.CSharp.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                platformInstructions = _promptProvider.GetPrompt("DotNet_Instructions.txt");
            }
            else if (targetLanguage.Equals(TargetLanguage.Java.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                platformInstructions = _promptProvider.GetPrompt("Java_Instructions.txt");
            }
            else
            {
                throw new NotSupportedException($"The target language '{targetLanguage}' is not supported.");
            }

            // 3. Replaces place holders
            string finalPrompt = promptTemplate
                .Replace("{PLATFORM_SPECIFIC_INSTRUCTIONS}", platformInstructions)
                .Replace("{SQL_CODE}", sqlContent)
                .Replace("{TARGET_LANGUAGE}", targetLanguage)
                .Replace("{AppName}", "Demo");

            return finalPrompt;
        }
    }
}
