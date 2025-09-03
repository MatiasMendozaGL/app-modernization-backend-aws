using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    public record MigrationDetailsRequest(string MigrationId, string UserId): IRequest<MigrationResponse>;
}
