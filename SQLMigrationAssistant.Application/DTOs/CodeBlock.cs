namespace SQLMigrationAssistant.Application.DTOs
{
    public record CodeBlock(
        string FilePath,
        string Code,
        string FileName
    );
}
