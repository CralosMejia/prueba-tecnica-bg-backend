namespace ShoppingCart.Domain.Carts;

public sealed class CartItem
{
    private CartItem()
    {
        // Constructor requerido por Entity Framework Core.
    }

    internal CartItem(
        Guid cartId,
        Guid productId,
        int quantity)
    {
        ValidateDataCartItem(cartId, productId, quantity);
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid CartId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }


    internal void IncreaseQuantity(int quantity)
    {
        ValidateQuantity(quantity);

        Quantity += quantity;
    }

    internal void UpdateQuantity(int quantity)
    {
        ValidateQuantity(quantity);

        Quantity = quantity;
    }

    public decimal CalculateSubtotal(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice),
                "Unit price cannot be negative."
            );
        }

        return unitPrice * Quantity;
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be positive."
            );
        }
    }

    private static void ValidateDataCartItem(Guid cartId, Guid productId, int quantity)
    {
        if (cartId == Guid.Empty)
        {
            throw new ArgumentException("Cart ID is required.", nameof(cartId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID is required.", nameof(productId));
        }
        ValidateQuantity(quantity);
    }
}