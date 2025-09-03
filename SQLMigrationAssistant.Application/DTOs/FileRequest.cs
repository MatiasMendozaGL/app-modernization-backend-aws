using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class FileRequest : IRequest<FileContentResponse>
    {
        public string MigrationId { get; set; }
        public string Filename { get; set; }
        public bool Download { get; set; }
        public string UserId { get; set; }
    }
}
