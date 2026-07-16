using ShoppingCart.Domain.Orders;

namespace ShoppingCart.UnitTests.Domain.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Create_WithValidLines_CreatesOrderWithItemsAndTotals()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var lines = new[]
        {
            new OrderLine(
                ProductId: Guid.NewGuid(),
                ProductCode: "PROD-001",
                ProductName: "Keyboard",
                UnitPrice: 50m,
                Quantity: 2
            ),
            new OrderLine(
                ProductId: Guid.NewGuid(),
                ProductCode: "PROD-002",
                ProductName: "Mouse",
                UnitPrice: 20m,
                Quantity: 1
            )
        };

        // Act
        var order = Order.Create(
            userId,
            lines,
            new DateTime(
                2026,
                7,
                15,
                12,
                0,
                0,
                DateTimeKind.Utc
            )
        );

        // Assert
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(2, order.Items.Count);

        Assert.Equal(120m, order.Subtotal);
        Assert.Equal(12m, order.Discount);
        Assert.Equal(108m, order.Total);
    }

    [Fact]
    public void Create_WhenSubtotalIsExactlyOneHundred_DoesNotApplyDiscount()
    {
        // Arrange
        var lines = new[]
        {
            new OrderLine(
                Guid.NewGuid(),
                "PROD-001",
                "Keyboard",
                UnitPrice: 50m,
                Quantity: 2
            )
        };

        // Act
        var order = Order.Create(
            Guid.NewGuid(),
            lines,
            DateTime.UtcNow
        );

        // Assert
        Assert.Equal(100m, order.Subtotal);
        Assert.Equal(0m, order.Discount);
        Assert.Equal(100m, order.Total);
    }

    [Fact]
    public void Create_WithNoItems_ThrowsInvalidOperationException()
    {
        // Act
        var action = () => Order.Create(
            Guid.NewGuid(),
            Array.Empty<OrderLine>(),
            DateTime.UtcNow
        );

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Create_StoresProductSnapshot()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var lines = new[]
        {
            new OrderLine(
                productId,
                "PROD-001",
                "Mechanical Keyboard",
                UnitPrice: 75m,
                Quantity: 2
            )
        };

        // Act
        var order = Order.Create(
            Guid.NewGuid(),
            lines,
            DateTime.UtcNow
        );

        // Assert
        var item = Assert.Single(order.Items);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal("PROD-001", item.ProductCode);
        Assert.Equal("Mechanical Keyboard", item.ProductName);
        Assert.Equal(75m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(150m, item.Subtotal);
    }
}