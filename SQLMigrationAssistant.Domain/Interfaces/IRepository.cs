namespace SQLMigrationAssistant.Domain.Interfaces
{
    public interface IRepository<T>
    {
        Task<T> SaveAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(string id);
    }
}
