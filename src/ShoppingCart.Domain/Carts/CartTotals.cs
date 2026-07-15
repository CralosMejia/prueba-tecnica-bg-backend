namespace ShoppingCart.Domain.Carts;

public record CartTotals(
    decimal Subtotal,
    decimal Discount,
    decimal Total
);