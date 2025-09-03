using FluentValidation;
using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.API.Validators
{
    public class DownloadFileRequestValidator : AbstractValidator<FileRequest>
    {
        public DownloadFileRequestValidator()
        {
            RuleFor(x => x.Filename)
                .NotEmpty()
                .WithMessage("Filename is required")
                .Must(BeValidFilename)
                .WithMessage("Invalid filename format");
        }

        private bool BeValidFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;

            var invalidChars = Path.GetInvalidFileNameChars();
            return !filename.Any(c => invalidChars.Contains(c));
        }
    }
}
