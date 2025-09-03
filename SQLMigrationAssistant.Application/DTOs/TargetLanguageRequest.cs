using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class TargetLanguageRequest : IRequest<IEnumerable<OptionResponse>>
    {
    }
}
