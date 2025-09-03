using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class LLMProviderRequest: IRequest<IEnumerable<OptionResponse>>
    {
    }
}
