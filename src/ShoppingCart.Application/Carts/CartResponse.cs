namespace ShoppingCart.Application.Carts;

public sealed record CartResponse(
    IReadOnlyCollection<CartItemResponse> Items,
    decimal Subtotal,
    decimal Discount,
    decimal Total
)
{
    public static CartResponse Empty { get; } =
        new(
            Items: Array.Empty<CartItemResponse>(),
            Subtotal: 0m,
            Discount: 0m,
            Total: 0m
        );
}