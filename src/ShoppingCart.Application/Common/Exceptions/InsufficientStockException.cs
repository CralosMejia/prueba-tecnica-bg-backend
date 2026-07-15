namespace ShoppingCart.Application.Common.Exceptions;

public sealed class InsufficientStockException
    : BusinessConflictException
{
    public InsufficientStockException(
        Guid productId,
        int requestedQuantity,
        int availableStock)
        : base(
            $"Product '{productId}' has insufficient stock. " +
            $"Requested: {requestedQuantity}. " +
            $"Available: {availableStock}."
        )
    {
    }
}