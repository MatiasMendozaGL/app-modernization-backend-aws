namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<bool> IsServiceAvailableAsync();

        /// <summary>
        /// Upload file content to GCP bucket
        /// </summary>
        /// <param name="content">file content to upload</param>
        /// <param name="fileName">Name for the file in the bucket</param>
        /// <param name="contentType">MIME type (defaults to text/plain)</param>
        /// <returns>The uploaded file name</returns>
        Task<string> UploadFileAsync(string content, string fileName, string contentType = "text/plain");

        /// <summary>
        /// Get files by a prefix in the bucket
        /// </summary>
        /// <param name="prefix">Optional prefix to filter files</param>
        /// <returns>List of file names</returns>
        Task<IEnumerable<string>> GetFilesByPrefixAsync(string? prefix);

        Task<byte[]> GetFileAsync(string fileName, CancellationToken cancellationToken = default);

        Task<byte[]> SearchAndGetFileAsync(string filename, CancellationToken cancellationToken = default);

        string GetContentType(string filename);

        Task DeleteDirectoryAsync(string directoryPath);
    }
}