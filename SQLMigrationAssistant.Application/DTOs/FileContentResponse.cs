namespace SQLMigrationAssistant.Application.DTOs
{
    public class FileContentResponse
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string Filename { get; set; }
        public string SignedUrl { get; set; }
    }
}
