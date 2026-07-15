namespace ShoppingCart.Domain.Carts;

public sealed class Cart
{
    private readonly List<CartItem> _items = [];

    private Cart()
    {
        // Constructor requerido por Entity Framework Core.
    }

    public Cart(Guid userId)
    {
        ValidateUserId(userId);
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public IReadOnlyCollection<CartItem> Items =>
        _items.AsReadOnly();

    public void AddItem(Guid productId, int quantity)
    {
        ValidateProductId(productId);
        ValidateQuantity(quantity);

        var existingItem = _items.FirstOrDefault(
            item => item.ProductId == productId
        );

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return;
        }

        var newItem = new CartItem(
            Id,
            productId,
            quantity
        );

        _items.Add(newItem);
    }

    public void UpdateQuantity(
        Guid productId,
        int quantity)
    {
        ValidateProductId(productId);
        ValidateQuantity(quantity);

        var item = FindItem(productId);

        item.UpdateQuantity(quantity);
    }

    public bool RemoveItem(Guid productId)
    {
        ValidateProductId(productId);

        var item = _items.FirstOrDefault(
            currentItem =>
                currentItem.ProductId == productId
        );

        if (item is null)
        {
            return false;
        }

        _items.Remove(item);

        return true;
    }

    public void Clear()
    {
        _items.Clear();
    }

    public CartTotals CalculateTotals(
        IReadOnlyDictionary<Guid, decimal> unitPrices)
    {
        ArgumentNullException.ThrowIfNull(unitPrices);

        var subtotal = _items.Sum(item =>
        {
            if (!unitPrices.TryGetValue(
                    item.ProductId,
                    out var unitPrice))
            {
                throw new KeyNotFoundException(
                    $"Price for product '{item.ProductId}' was not provided."
                );
            }

            return item.CalculateSubtotal(unitPrice);
        });

        var discount = subtotal > 100m
            ? subtotal * 0.10m
            : 0m;

        var total = subtotal - discount;

        return new CartTotals(
            Subtotal: subtotal,
            Discount: discount,
            Total: total
        );
    }

    private CartItem FindItem(Guid productId)
    {
        return _items.FirstOrDefault(
            item => item.ProductId == productId
        ) ?? throw new KeyNotFoundException(
            $"Product '{productId}' does not exist in the cart."
        );
    }

    private static void ValidateProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product identifier is required.",
                nameof(productId)
            );
        }
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

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier is required.",
                nameof(userId)
            );
        }
    }
}