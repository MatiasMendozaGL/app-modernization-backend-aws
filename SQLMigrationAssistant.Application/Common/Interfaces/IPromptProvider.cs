namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IPromptProvider
    {
        string GetPrompt(string promptName);
    }
}
