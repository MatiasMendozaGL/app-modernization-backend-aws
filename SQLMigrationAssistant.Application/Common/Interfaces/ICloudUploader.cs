using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface ICloudUploader
    {
        Task<string> UploadCodeBlocks(IEnumerable<CodeBlock> codeBlocks);
    }
}
