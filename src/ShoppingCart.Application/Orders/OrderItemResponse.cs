namespace ShoppingCart.Application.Orders;

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);