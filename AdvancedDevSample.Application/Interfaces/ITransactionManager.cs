namespace AdvancedDevSample.Application.Interfaces;

public interface ITransactionManager
{
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
