using ShoppingCart.Domain.Orders;

namespace ShoppingCart.Application.Orders;

public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Order>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<Order?> GetByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}