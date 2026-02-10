using AdvancedDevSample.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AdvancedDevSample.Infrastructure.Persistence;

public class EfTransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;

    public EfTransactionManager(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.CurrentTransaction != null || !_dbContext.Database.IsRelational())
        {
            await action();
            return;
        }

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await action();
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.CurrentTransaction != null || !_dbContext.Database.IsRelational())
        {
            return await action();
        }

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        T result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
