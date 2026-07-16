using Microsoft.EntityFrameworkCore;
using ShoppingCart.Application.Orders;
using ShoppingCart.Domain.Orders;
using ShoppingCart.Infrastructure.Persistence;

namespace ShoppingCart.Infrastructure.Orders;

public sealed class OrderRepository(
    ShoppingCartDbContext dbContext)
    : IOrderRepository
{
    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        await dbContext.Orders.AddAsync(
            order,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order =>
                    order.Id == orderId &&
                    order.UserId == userId,
                cancellationToken
            );
    }
}