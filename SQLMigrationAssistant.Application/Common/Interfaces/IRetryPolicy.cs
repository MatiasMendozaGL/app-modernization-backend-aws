namespace SQLMigrationAssistant.Application.Common.Interfaces;

public interface IRetryPolicy
{

    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation);

    public Task ExecuteAsync(Func<Task> operation);
    
}