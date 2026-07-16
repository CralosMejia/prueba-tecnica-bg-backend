namespace ShoppingCart.Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<OrderItemResponse> Items,
    decimal Subtotal,
    decimal Discount,
    decimal Total
);