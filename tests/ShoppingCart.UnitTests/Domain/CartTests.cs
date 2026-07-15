using ShoppingCart.Domain.Carts;

namespace ShoppingCart.UnitTests.Domain
{
    public class CartTests
    {

        [Fact]
        public void CalculateTotals_WhenSubtotalExceedsOneHundred_AppliesTenPercentDiscount()
        {
            // Arrange
            var cart = new Cart();
            cart.AddItem(unitPrice: 60m, quantity: 2);

            // Act
            var totals = cart.CalculateTotals();

            // Assert
            Assert.Equal(120m, totals.Subtotal);
            Assert.Equal(12m, totals.Discount);
            Assert.Equal(108m, totals.Total);
        }


        [Fact]
        public void CalculateTotals_WhenSubtotalIsExactlyOneHundred_DoesNotApplyDiscount()
        {
            // Arrange
            var cart = new Cart();
            cart.AddItem(unitPrice: 50m, quantity: 2);

            // Act
            var totals = cart.CalculateTotals();

            // Assert
            Assert.Equal(100m, totals.Subtotal);
            Assert.Equal(0m, totals.Discount);
            Assert.Equal(100m, totals.Total);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddItem_WhenQuantityIsNotPositive_ThrowsArgumentOutOfRangeException(
            int invalidQuantity)
        {
            // Arrange
            var cart = new Cart();

            // Act
            var action = () => cart.AddItem(
                unitPrice: 20m,
                quantity: invalidQuantity
            );

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(action);
        }
    }
}