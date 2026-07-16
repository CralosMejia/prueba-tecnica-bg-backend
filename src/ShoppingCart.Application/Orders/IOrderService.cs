namespace ShoppingCart.Application.Orders;

public interface IOrderService
{
    Task<OrderResponse> CheckoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<OrderResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<OrderResponse> GetByIdAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken = default
    );
}