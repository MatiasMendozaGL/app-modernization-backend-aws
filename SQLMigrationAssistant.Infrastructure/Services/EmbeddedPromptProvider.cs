using SQLMigrationAssistant.Application.Common.Interfaces;
using System.Reflection;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class EmbeddedPromptProvider: IPromptProvider
    {
        public string GetPrompt(string promptName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = $"SQLMigrationAssistant.Infrastructure.Prompts.{promptName}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException($"The prompt '{promptName}' was not found as embedded resource'.");
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
