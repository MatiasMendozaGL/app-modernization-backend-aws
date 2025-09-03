namespace SQLMigrationAssistant.Application.DTOs
{
    /// <summary>
    /// Response containing the ZIP file data and metadata
    /// </summary>
    public class ZipFileResponse
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/zip";
        public string FileName { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
    }
}
