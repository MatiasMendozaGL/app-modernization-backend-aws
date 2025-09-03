using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IMigrationService
    {
        Task<MigrationResponse> GetMigrationDetailsAsync(string migrationId, string userId);
        Task<IEnumerable<string>> GetGeneratedFileNamesAsync(string userId, string migrationId);
        Task<string> GetSourceFileNameAsync(string userId, string migrationId);
        Task CleanupMigrationArtifactsAsync(string userId, string migrationId);
    }
}
