using MediatR;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;
using System.IO.Compression;

namespace SQLMigrationAssistant.Application.Handlers
{
    public class DownloadAllFilesHandler : IRequestHandler<DownloadAllFilesRequest, ZipFileResponse?>
    {
        private readonly IMigrationService _migrationService;
        private readonly IZipService _zipService;
        private readonly ILogger<DownloadAllFilesHandler> _logger;

        public DownloadAllFilesHandler(
            IMigrationService migrationService,
            IZipService zipService,
            ILogger<DownloadAllFilesHandler> logger)
        {
            _migrationService = migrationService;
            _zipService = zipService;
            _logger = logger;
        }

        public async Task<ZipFileResponse?> Handle(DownloadAllFilesRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing download all files request for migration {MigrationId}, user {UserId}",
                request.MigrationId, request.UserId);

            try
            {
                var migration = await _migrationService.GetMigrationDetailsAsync(request.MigrationId, request.UserId);

                if (migration.GeneratedCodeFiles == null || !migration.GeneratedCodeFiles.Any())
                {
                    _logger.LogWarning("No generated files found for migration {MigrationId}", request.MigrationId);
                    return null;
                }

                var result = await _zipService.CreateZipFileAsync(
                    request.UserId,
                    request.MigrationId,
                    migration.GeneratedCodeFiles,
                    cancellationToken);

                if (result != null)
                {
                    _logger.LogInformation("Successfully created ZIP file for migration {MigrationId}", request.MigrationId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZIP file for migration {MigrationId}", request.MigrationId);
                throw;
            }
        }
    }
}