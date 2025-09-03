namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface ICodeBlockProcessor
    {
        Task<IEnumerable<string>> ProcessCodeBlocksAsync(
            string llmOutput,
            string userId,
            string migrationId,
            CancellationToken cancellationToken);
    }
}
