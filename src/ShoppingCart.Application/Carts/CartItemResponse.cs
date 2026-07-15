namespace ShoppingCart.Application.Carts;

public sealed record CartItemResponse(
    Guid ProductId,
    string Code,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);