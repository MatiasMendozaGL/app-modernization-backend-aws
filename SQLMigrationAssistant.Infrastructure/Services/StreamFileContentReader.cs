using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class StreamFileContentReader : IFileContentReader
    {
        private readonly ILogger<StreamFileContentReader> _logger;

        public StreamFileContentReader(ILogger<StreamFileContentReader> logger)
        {
            _logger = logger;
        }

        public async Task<string> ReadAsync(Stream content, CancellationToken cancellationToken = default)
        {
            try
            {
                if (content == null)
                    throw new ArgumentNullException(nameof(content));

                // Reset stream position
                if (content.CanSeek)
                    content.Position = 0;

                using var reader = new StreamReader(content, leaveOpen: true);
                var fileContent = await reader.ReadToEndAsync(cancellationToken);

                _logger.LogDebug("Successfully read {CharacterCount} characters from stream", fileContent.Length);
                return fileContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file content from stream");
                throw;
            }
        }
    }
}
