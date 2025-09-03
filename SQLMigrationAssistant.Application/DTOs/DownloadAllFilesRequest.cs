using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    /// <summary>
    /// Request to download all files for a migration as a ZIP
    /// </summary>
    public class DownloadAllFilesRequest : IRequest<ZipFileResponse?>
    {
        public string MigrationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
    
}