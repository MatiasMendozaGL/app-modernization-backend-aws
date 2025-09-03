// 1. Create a shared service for migration operations
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Exceptions;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Entities;
using SQLMigrationAssistant.Domain.Interfaces;

namespace SQLMigrationAssistant.Application.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly IMigrationRepository<Migration> _migrationRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(
            IMigrationRepository<Migration> migrationRepository,
            IFileStorageService fileStorageService,
            ILogger<MigrationService> logger)
        {
            _migrationRepository = migrationRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<MigrationResponse> GetMigrationDetailsAsync(string migrationId, string userId)
        {
            _logger.LogInformation("Fetching details for migration ID: {MigrationId}", migrationId);

            var migration = await _migrationRepository.FindByMigrationIdAndUserIdAsync(migrationId, userId);
            if (migration == null)
            {
                _logger.LogWarning("Migration with ID {MigrationId} not found.", migrationId);
                throw new MigrationException(migrationId, $"Migration with ID '{migrationId}' not found.");
            }

            var generatedFiles = await GetGeneratedFileNamesAsync(userId, migrationId);

            _logger.LogInformation("Retrieved {Count} files for migration ID: {MigrationId}.", generatedFiles.Count(), migrationId);

            return new MigrationResponse
            {
                MigrationId = migrationId,
                LLMModel = migration.LLMModel,
                LastMigrationExecution = migration.LastMigrationExecution,
                Status = migration.Status,
                TargetLanguage = migration.TargetLanguage,
                SourceFileName = migration.SourceFileName,
                GeneratedCodeFiles = generatedFiles
            };
        }

        public async Task<IEnumerable<string>> GetGeneratedFileNamesAsync(string userId, string migrationId)
        {
            var prefix = $"{userId}/{migrationId}/";
            var allFileNames = await _fileStorageService.GetFilesByPrefixAsync(prefix);
            return allFileNames
                .Select(x => x.Replace(prefix, string.Empty))
                    .Where(x => !x.StartsWith("source_") && !x.Equals("migration.json"));
        }

        public async Task<string> GetSourceFileNameAsync(string userId, string migrationId)
        {
            var prefix = $"{userId}/{migrationId}/";
            var allFileNames = await _fileStorageService.GetFilesByPrefixAsync(prefix);
            var sourceFileName = allFileNames
                .Select(Path.GetFileName)
                .FirstOrDefault(x => x.StartsWith("source_"))
                ?.Replace("source_", "") ?? string.Empty;

            return sourceFileName;
        }

        public async Task CleanupMigrationArtifactsAsync(string userId, string migrationId)
        {
            _logger.LogWarning("Cleaning up artifacts for migration: {MigrationId}", migrationId);
            try
            {
                var directoryPath = $"{userId}/{migrationId}/";
                await _fileStorageService.DeleteDirectoryAsync(directoryPath);
                _logger.LogInformation("Successfully cleaned up artifacts for migration: {MigrationId}", migrationId);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to clean up artifacts for migration: {MigrationId}", migrationId);
                throw;
            }
        }
    }
}