namespace ShoppingCart.Domain.Carts;

public class Cart
{

    private readonly List<(decimal UnitPrice, int Quantity)> _items = new();
    public void AddItem(decimal unitPrice, int quantity)
    {
        ValidateQuantity(quantity);
        _items.Add((UnitPrice: unitPrice, Quantity: quantity));
    }

    public void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }
    }

    public CartTotals CalculateTotals()
    {
        var subtotal = _items.Sum(item => item.UnitPrice * item.Quantity);
        var discount = subtotal > 100 ? subtotal * 0.1m : 0;
        var total = subtotal - discount;

        return new CartTotals(
            Subtotal: subtotal,
            Discount: discount,
            Total: total
        );
    }
}