using SQLMigrationAssistant.Domain.Entities;

namespace SQLMigrationAssistant.Domain.Interfaces
{
    public interface IMigrationRepository<T> : IRepository<T>
    {
        Task<IEnumerable<Migration>> FindByUserIdAsync(string userId);

        Task<Migration?> FindByMigrationIdAndUserIdAsync(string migrationId, string userId);

    }
}
