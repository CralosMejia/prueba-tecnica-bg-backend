using System.Data;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Application.Common.Persistence;

namespace ShoppingCart.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    ShoppingCartDbContext dbContext)
    : IUnitOfWork
{
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var executionStrategy =
            dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await dbContext.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable,
                        cancellationToken
                    );

                try
                {
                    var result = await operation(
                        cancellationToken
                    );

                    await dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(
                        CancellationToken.None
                    );

                    /*
                     * Evita conservar entidades modificadas en memoria
                     * si la estrategia vuelve a ejecutar la operación.
                     */
                    dbContext.ChangeTracker.Clear();

                    throw;
                }
            }
        );
    }
}