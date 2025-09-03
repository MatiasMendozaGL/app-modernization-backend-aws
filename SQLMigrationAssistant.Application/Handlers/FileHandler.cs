using MediatR;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.Application.Handlers
{
    public class FileHandler : IRequestHandler<FileRequest, FileContentResponse>
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileHandler> _logger;

        public FileHandler(
            IFileStorageService fileStorageService,
            ILogger<FileHandler> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<FileContentResponse> Handle(FileRequest request, CancellationToken cancellationToken)
        {
            var action = request.Download ? "download" : "get";
            _logger.LogInformation("Processing {Action} file request. Filename: {Filename}, UserId: {UserId}",
                action, request.Filename, request.UserId);

            try
            {
                var fileName = $"{request.UserId}/{request.MigrationId}/{request.Filename}";
                var fileContent = await _fileStorageService.SearchAndGetFileAsync(fileName, cancellationToken);
                var contentType = request.Download
                    ? _fileStorageService.GetContentType(request.Filename)
                    : "text/plain";

                _logger.LogInformation("Successfully processed {Action} request. Filename: {Filename}, Size: {Size} bytes",
                    action, request.Filename, fileContent.Length);

                return new FileContentResponse
                {
                    Content = fileContent,
                    ContentType = contentType,
                    Filename = request.Filename
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {Action} request for file {Filename}", action, request.Filename);
                throw;
            }
        }
    }
}
