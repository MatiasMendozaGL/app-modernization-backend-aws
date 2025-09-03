using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class CodeBlockProcessor : ICodeBlockProcessor
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ICodeBlockExtractor _codeBlockExtractor;
        private readonly ILogger<CodeBlockProcessor> _logger;

        public CodeBlockProcessor(
            IFileStorageService fileStorageService,
            ICodeBlockExtractor codeBlockExtractor,
            ILogger<CodeBlockProcessor> logger)
        {
            _fileStorageService = fileStorageService;
            _codeBlockExtractor = codeBlockExtractor;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> ProcessCodeBlocksAsync(
            string llmOutput,
            string userId,
            string migrationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(llmOutput))
                {
                    _logger.LogWarning("No LLM output to process for migration {MigrationId}", migrationId);
                    return [];
                }

                var codeBlocks = _codeBlockExtractor.Extract(llmOutput);

                if (!codeBlocks.Any())
                {
                    _logger.LogWarning("No code blocks found in LLM output for migration {MigrationId}", migrationId);
                    return [];
                }

                var uploadedFileNames = new System.Collections.Concurrent.ConcurrentBag<string>();
                var uploadTasks = codeBlocks.Select(async block =>
                {
                    var codeFileName = $"{userId}/{migrationId}/{block.FilePath}";
                    await _fileStorageService.UploadFileAsync(block.Code, codeFileName);
                    uploadedFileNames.Add(block.FileName);
                    _logger.LogDebug("Uploaded code block {FileName} for migration {MigrationId}",
                        block.FileName, migrationId);
                });

                await Task.WhenAll(uploadTasks);

                _logger.LogInformation("Successfully processed all code blocks for migration {MigrationId}", migrationId);
                return uploadedFileNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing code blocks for migration {MigrationId}", migrationId);
                throw;
            }
        }
    }
}
