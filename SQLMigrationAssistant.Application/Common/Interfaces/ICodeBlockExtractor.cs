using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface ICodeBlockExtractor
    {
        /// <summary>
        /// Extracts code blocks from the given content
        /// </summary>
        /// <param name="content">Content to parse</param>
        /// <returns>Collection of extracted code blocks</returns>
        IEnumerable<CodeBlock> Extract(string content);
    }
}
