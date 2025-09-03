using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;
using System.IO.Compression;

namespace SQLMigrationAssistant.Application.Services
{
    public class ZipService : IZipService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ZipService> _logger;

        public ZipService(IFileStorageService fileStorageService, ILogger<ZipService> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ZipFileResponse?> CreateZipFileAsync(string userId, string migrationId, IEnumerable<string> fileNames, CancellationToken cancellationToken = default)
        {
            if (!fileNames.Any())
            {
                _logger.LogWarning("No files provided for ZIP creation for migration {MigrationId}", migrationId);
                return null;
            }

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var (fileCount, totalSize) = await AddFilesToArchiveAsync(archive, fileNames, userId, migrationId, cancellationToken);

                if (fileCount == 0)
                {
                    _logger.LogWarning("No files were successfully added to ZIP for migration {MigrationId}", migrationId);
                    return null;
                }

                _logger.LogInformation("Successfully created ZIP with {FileCount} files (total size: {TotalSize} bytes) for migration {MigrationId}",
                    fileCount, totalSize, migrationId);
            }

            return CreateZipResponse(memoryStream.ToArray(), migrationId, fileNames.Count());
        }

        private async Task<(int fileCount, long totalSize)> AddFilesToArchiveAsync(
            ZipArchive archive,
            IEnumerable<string> fileNames,
            string userId,
            string migrationId,
            CancellationToken cancellationToken)
        {
            int fileCount = 0;
            long totalSize = 0;

            foreach (var fileName in fileNames)
            {
                try
                {
                    var filePath = $"{userId}/{migrationId}/{fileName}";
                    var fileContent = await _fileStorageService.GetFileAsync(filePath, cancellationToken);

                    if (fileContent?.Length > 0)
                    {
                        var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(fileContent, 0, fileContent.Length, cancellationToken);

                        fileCount++;
                        totalSize += fileContent.Length;

                        _logger.LogDebug("Added file {FileName} to ZIP (size: {Size} bytes)", fileName, fileContent.Length);
                    }
                    else
                    {
                        _logger.LogWarning("File {FileName} is empty or not found in storage", fileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding file {FileName} to ZIP archive", fileName);
                }
            }

            return (fileCount, totalSize);
        }

        private static ZipFileResponse CreateZipResponse(byte[] zipContent, string migrationId, int fileCount)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
            var zipFileName = $"migration-{migrationId}-files-{timestamp}.zip";

            return new ZipFileResponse
            {
                Content = zipContent,
                ContentType = "application/zip",
                FileName = zipFileName,
                FileCount = fileCount,
                TotalSize = zipContent.Length
            };
        }
    }
}
