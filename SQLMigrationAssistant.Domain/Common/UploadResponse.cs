namespace SQLMigrationAssistant.Domain.Common
{
    public class UploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
    }
}
