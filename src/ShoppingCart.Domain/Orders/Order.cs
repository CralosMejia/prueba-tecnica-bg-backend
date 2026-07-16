namespace ShoppingCart.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
        // Requerido por Entity Framework Core.
    }

    private Order(
        Guid userId,
        DateTime createdAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier is required.",
                nameof(userId)
            );
        }

        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal Discount { get; private set; }

    public decimal Total { get; private set; }

    public IReadOnlyCollection<OrderItem> Items =>
        _items.AsReadOnly();

    public static Order Create(
        Guid userId,
        IEnumerable<OrderLine> lines,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var lineList = lines.ToList();

        if (lineList.Count == 0)
        {
            throw new InvalidOperationException(
                "An order must contain at least one item."
            );
        }

        var order = new Order(
            userId,
            createdAtUtc
        );

        foreach (var line in lineList)
        {
            order._items.Add(
                new OrderItem(order.Id, line)
            );
        }

        order.Subtotal = order._items.Sum(
            item => item.Subtotal
        );

        order.Discount = order.Subtotal > 100m
            ? order.Subtotal * 0.10m
            : 0m;

        order.Total =
            order.Subtotal - order.Discount;

        return order;
    }
}