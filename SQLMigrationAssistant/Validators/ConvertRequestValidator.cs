using FluentValidation;
using SQLMigrationAssistant.API.Models;

namespace SQLMigrationAssistant.API.Validators
{
    public class ConvertRequestValidator : AbstractValidator<ConvertRequest>
    {
        public ConvertRequestValidator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage("No file provided.")
                .Must(BeAValidFile).WithMessage("The provided file is empty or invalid.");

            RuleFor(x => x.File.FileName)
                .Must(HaveSqlExtension)
                .WithMessage("Only .sql files are allowed for migration");
        }

        private bool BeAValidFile(IFormFile? file) => file != null && file.Length > 0;
        private bool HaveSqlExtension(string fileName) => !string.IsNullOrEmpty(fileName)
            && Path.GetExtension(fileName).Equals(".sql", StringComparison.CurrentCultureIgnoreCase);
    }
}
