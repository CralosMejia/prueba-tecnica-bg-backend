namespace ShoppingCart.Domain.Orders;

public sealed class OrderItem
{
    private OrderItem()
    {
        // Requerido por Entity Framework Core.
    }

    internal OrderItem(
        Guid orderId,
        OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order identifier is required.",
                nameof(orderId)
            );
        }

        if (line.ProductId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product identifier is required.",
                nameof(line)
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            line.ProductCode
        );

        ArgumentException.ThrowIfNullOrWhiteSpace(
            line.ProductName
        );

        if (line.UnitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(line),
                "Unit price cannot be negative."
            );
        }

        if (line.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(line),
                "Quantity must be greater than zero."
            );
        }

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = line.ProductId;
        ProductCode = line.ProductCode.Trim();
        ProductName = line.ProductName.Trim();
        UnitPrice = line.UnitPrice;
        Quantity = line.Quantity;
        Subtotal = UnitPrice * Quantity;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } =
        string.Empty;

    public string ProductName { get; private set; } =
        string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal Subtotal { get; private set; }
}