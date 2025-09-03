using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Entities;
using SQLMigrationAssistant.Domain.Interfaces;

namespace SQLMigrationAssistant.Application.Handlers
{
    public class MigrationListHandler : IRequestHandler<MigrationListRequest, IEnumerable<MigrationResponse>>
    {
        private readonly IMigrationRepository<Migration> _migrationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MigrationListHandler> _logger;

        public MigrationListHandler(
            IMigrationRepository<Migration> migrationRepository,
            IMapper mapper,
            ILogger<MigrationListHandler> logger)
        {
            _migrationRepository = migrationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<MigrationResponse>> Handle(MigrationListRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting migrations for user {UserId}", request.UserId);

            try
            {
                var migrations = await _migrationRepository.FindByUserIdAsync(request.UserId);
                var migrationResponses = _mapper.Map<IEnumerable<MigrationResponse>>(migrations);

                var sortedMigrations = migrationResponses.OrderByDescending(m =>
                    DateTime.TryParse(m.LastMigrationExecution, out var date) ? date : DateTime.MinValue);

                _logger.LogInformation("Successfully retrieved {Count} migrations for user {UserId}",
                    sortedMigrations.Count(), request.UserId);

                return sortedMigrations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving migrations for user {UserId}", request.UserId);
                throw;
            }
        }
    }
}
