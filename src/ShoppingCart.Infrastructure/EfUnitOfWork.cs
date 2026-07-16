using ShoppingCart.Application.Common.Persistence;

namespace ShoppingCart.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
    ShoppingCartDbContext dbContext)
    : IUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}