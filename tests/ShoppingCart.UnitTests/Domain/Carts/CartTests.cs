using ShoppingCart.Domain.Carts;

namespace ShoppingCart.UnitTests.Domain.Carts;

public sealed class CartTests
{
    [Fact]
    public void Constructor_WithValidUserId_CreatesEmptyCart()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var cart = new Cart(userId);

        // Assert
        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.Equal(userId, cart.UserId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act
        var action = () => new Cart(Guid.Empty);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void AddItem_WithValidData_AddsProductToCart()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, quantity: 2);

        // Assert
        var item = Assert.Single(cart.Items);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void AddItem_WhenProductAlreadyExists_IncreasesQuantity()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();

        cart.AddItem(productId, quantity: 2);

        // Act
        cart.AddItem(productId, quantity: 3);

        // Assert
        var item = Assert.Single(cart.Items);

        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void UpdateQuantity_WithExistingProduct_ReplacesQuantity()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();

        cart.AddItem(productId, quantity: 2);

        // Act
        cart.UpdateQuantity(productId, quantity: 6);

        // Assert
        var item = Assert.Single(cart.Items);

        Assert.Equal(6, item.Quantity);
    }

    [Fact]
    public void UpdateQuantity_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid());

        // Act
        var action = () =>
            cart.UpdateQuantity(Guid.NewGuid(), quantity: 2);

        // Assert
        Assert.Throws<KeyNotFoundException>(action);
    }

    [Fact]
    public void RemoveItem_WithExistingProduct_RemovesItem()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();

        cart.AddItem(productId, quantity: 2);

        // Act
        var wasRemoved = cart.RemoveItem(productId);

        // Assert
        Assert.True(wasRemoved);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Clear_WithItems_RemovesAllItems()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid());

        cart.AddItem(Guid.NewGuid(), quantity: 2);
        cart.AddItem(Guid.NewGuid(), quantity: 3);

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void CalculateTotals_WhenSubtotalIsGreaterThanOneHundred_AppliesDiscount()
    {
        // Arrange
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();

        var cart = new Cart(Guid.NewGuid());

        cart.AddItem(firstProductId, quantity: 2);
        cart.AddItem(secondProductId, quantity: 1);

        var unitPrices = new Dictionary<Guid, decimal>
        {
            [firstProductId] = 50m,
            [secondProductId] = 20m
        };

        // Act
        var totals = cart.CalculateTotals(unitPrices);

        // Assert
        Assert.Equal(120m, totals.Subtotal);
        Assert.Equal(12m, totals.Discount);
        Assert.Equal(108m, totals.Total);
    }

    [Fact]
    public void CalculateTotals_WhenSubtotalIsExactlyOneHundred_DoesNotApplyDiscount()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cart = new Cart(Guid.NewGuid());

        cart.AddItem(productId, quantity: 2);

        var unitPrices = new Dictionary<Guid, decimal>
        {
            [productId] = 50m
        };

        // Act
        var totals = cart.CalculateTotals(unitPrices);

        // Assert
        Assert.Equal(100m, totals.Subtotal);
        Assert.Equal(0m, totals.Discount);
        Assert.Equal(100m, totals.Total);
    }
    [Fact]
    public void CalculateTotals_WithSingleProduct_CalculatesItemSubtotal()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cart = new Cart(Guid.NewGuid());

        cart.AddItem(productId, quantity: 3);

        var unitPrices = new Dictionary<Guid, decimal>
        {
            [productId] = 25m
        };

        // Act
        var totals = cart.CalculateTotals(unitPrices);

        // Assert
        Assert.Equal(75m, totals.Subtotal);
        Assert.Equal(0m, totals.Discount);
        Assert.Equal(75m, totals.Total);
    }
}