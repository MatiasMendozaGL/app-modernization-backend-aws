using MediatR;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.Application.Handlers
{
    public class TargetLanguagesHandler : IRequestHandler<TargetLanguageRequest, IEnumerable<OptionResponse>>
    {
        public Task<IEnumerable<OptionResponse>> Handle(TargetLanguageRequest request, CancellationToken cancellationToken)
        {
            var providers = Enum.GetValues(typeof(TargetLanguage))
                            .Cast<TargetLanguage>()
                            .Select(p => new OptionResponse
                            {
                                Id = (int)p,
                                Description = p.ToString()
                            });

            return Task.FromResult(providers);
        }
    }
}
