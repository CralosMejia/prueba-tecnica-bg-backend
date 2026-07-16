namespace ShoppingCart.Domain.Orders;

public sealed record OrderLine(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);