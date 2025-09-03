using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    public record MigrationListRequest(string UserId) : IRequest<IEnumerable<MigrationResponse>>;
}
