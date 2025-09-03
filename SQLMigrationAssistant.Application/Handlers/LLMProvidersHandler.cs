using MediatR;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Application.Handlers
{
    public class LLMProvidersHandler : IRequestHandler<LLMProviderRequest, IEnumerable<OptionResponse>>
    {
        public Task<IEnumerable<OptionResponse>> Handle(LLMProviderRequest request, CancellationToken cancellationToken)
        {
            var providers = Enum.GetValues(typeof(LLMProviderType))
                                .Cast<LLMProviderType>()
                                .Select(p => new OptionResponse
                                {
                                    Id = (int)p,
                                    Description = p.ToString()
                                });

            return Task.FromResult(providers);
        }
    }
}
