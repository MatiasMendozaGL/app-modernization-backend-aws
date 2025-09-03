using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IZipService
    {
        Task<ZipFileResponse?> CreateZipFileAsync(string userId, string migrationId, IEnumerable<string> fileNames, CancellationToken cancellationToken = default);
    }
}
