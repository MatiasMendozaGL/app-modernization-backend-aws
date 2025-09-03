using MediatR;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Exceptions;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Entities;
using SQLMigrationAssistant.Domain.Interfaces;

namespace SQLMigrationAssistant.Application.Handlers
{
    public class MigrationDetailsHandler : IRequestHandler<MigrationDetailsRequest, MigrationResponse>
    {
        private readonly IMigrationService _migrationService;
        private readonly ILogger<MigrationDetailsHandler> _logger;

        public MigrationDetailsHandler(
            IMigrationService migrationService,
            ILogger<MigrationDetailsHandler> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        public async Task<MigrationResponse> Handle(MigrationDetailsRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing migration details request for migration {MigrationId}, user {UserId}",
                request.MigrationId, request.UserId);

            try
            {
                var result = await _migrationService.GetMigrationDetailsAsync(request.MigrationId, request.UserId);
                _logger.LogInformation("Successfully retrieved migration details for {MigrationId}", request.MigrationId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving migration details for {MigrationId}", request.MigrationId);
                throw;
            }
        }
    }
}
