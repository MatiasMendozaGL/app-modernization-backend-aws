using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Exceptions;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;
using System.Text.RegularExpressions;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class LLMCodeBlockExtractor : ICodeBlockExtractor
    {
        private readonly ILogger<LLMCodeBlockExtractor> _logger;

        // It uses [a-z]* to match ANY language specifier (xml, json, csharp) or none at all.
        private static readonly Regex CodeBlockPattern = new(
            @"--START NEW BLOCK FOR ([^-]+)--\s*```[a-z]*\s*(.*?)\s*```\s*--END NEW BLOCK FOR \1--",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public LLMCodeBlockExtractor(ILogger<LLMCodeBlockExtractor> logger)
        {
            _logger = logger;
        }

        public IEnumerable<CodeBlock> Extract(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogDebug("No content provided for code block extraction");
                return [];
            }

            try
            {
                var blocks = new List<CodeBlock>();
                var matches = CodeBlockPattern.Matches(content);

                _logger.LogInformation("Found {MatchCount} code blocks to extract.", matches.Count);

                foreach (Match match in matches)
                {
                    var filePath = match.Groups[1].Value.Trim();
                    var sourceCode = match.Groups[2].Value.Trim();

                    var fileName = Path.GetFileName(filePath);

                    blocks.Add(new CodeBlock(filePath, sourceCode, fileName));

                    _logger.LogDebug("Extracted file: {FilePath}", filePath);
                }
                return blocks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while extracting code blocks from content");
                throw new MigrationException("Failed to extract code blocks from LLM output", ex.Message);
            }
        }
    }
}
