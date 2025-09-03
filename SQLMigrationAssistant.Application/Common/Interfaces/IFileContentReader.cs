namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IFileContentReader
    {
        Task<string> ReadAsync(Stream content, CancellationToken cancellationToken);
    }
}
