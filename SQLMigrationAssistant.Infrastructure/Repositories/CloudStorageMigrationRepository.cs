using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Exceptions;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain.Entities;
using SQLMigrationAssistant.Domain.Interfaces;
using System.Text.Json;
using System.Threading;

namespace SQLMigrationAssistant.Infrastructure.Repositories
{
    public class CloudStorageMigrationRepository : IMigrationRepository<Migration>
    {
        private readonly IFileStorageService _fileManagerService;
        private readonly ILogger<CloudStorageMigrationRepository> _logger;

        public CloudStorageMigrationRepository(ILogger<CloudStorageMigrationRepository> logger, IFileStorageService fileManagerService)
        {
            _logger = logger;
            _fileManagerService = fileManagerService;
        }

        public async Task<Migration> SaveAsync(Migration entity)
        {
            try
            {
                string json = JsonSerializer.Serialize(entity);
                var fileName = $"{entity.UserId}/{entity.MigrationId}/migration.json";
                var fileInfo = await _fileManagerService.UploadFileAsync(json, fileName, "application/json");
                _logger.LogInformation("Migration {MigrationId} created successfully for user {UserId}", entity.MigrationId, entity.UserId);
                return entity;

            }
            catch (IOException ex)
            {
                _logger.LogInformation(ex, "Failed to create migration {MigrationId} for user {UserId}", entity.UserId, entity.MigrationId);
                throw new MigrationException("Migration was not created.", ex.Message);
            }
        }

        public async Task<IEnumerable<Migration>> GetAllAsync()
        {
            throw new NotSupportedException();
        }

        public async Task<Migration> GetByIdAsync(string id)
        {
            throw new NotSupportedException();
        }

        public async Task<Migration?> FindByMigrationIdAndUserIdAsync(string migrationId, string userId)
        {
            var fileName = $"{userId}/{migrationId}/migration.json";

            try
            {
                byte[] migrationBytes = await _fileManagerService.SearchAndGetFileAsync(fileName);
                var migration = JsonSerializer.Deserialize<Migration>(migrationBytes);
                if (migration == null)
                {
                    _logger.LogWarning("The migration file {FileName}' is empty or is null", fileName);
                    return null;
                }
                return migration;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize the migration file {FileName}", fileName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving migration for user '{UserId}'", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Migration>> FindByUserIdAsync(string userId)
        {
            var migrations = new List<Migration>();

            try
            {
                IEnumerable<byte[]> migrationFiles = await GetMigrationFilesAsync(userId);

                foreach (var migrationBytes in migrationFiles)
                {
                    try
                    {
                        var migration = JsonSerializer.Deserialize<Migration>(migrationBytes);
                        if (migration != null)
                        {
                            migrations.Add(migration);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize a migration file for user {UserId}.", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving migrations for user '{UserId}'", userId);
                throw new MigrationException("Failed to retrieve migrations", ex.Message);
            }
            return migrations;
        }

        private async Task<IEnumerable<byte[]>> GetMigrationFilesAsync(string path)
        {
            var migrationFiles = new List<byte[]>();
            try
            {
                var files = await _fileManagerService.GetFilesByPrefixAsync(path + "/");

                foreach (var file in files)
                {
                    if (file.EndsWith("migration.json"))
                    {
                        try
                        {
                            var fileContent = await _fileManagerService.GetFileAsync(file);
                            migrationFiles.Add(fileContent);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error getting the content of file {FileName}", file);
                        }
                    }
                }
                return migrationFiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing migration files for path {Path}", path);
                throw new MigrationException("The migration files could not be accessed", ex.Message);
            }
        }

    }
}
