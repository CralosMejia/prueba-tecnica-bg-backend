using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Domain
{
    public class ProductTests
    {
        [Fact]
        public void Constructor_WhenPriceIsNegative_ThrowsArgumentOutOfRangeException(){
            // Arrange
            const decimal invalidPrice = -10m;

            // Act
            var action = () => new Product(
                code: "PRD-001",
                name: "Mechanical Keyboard",
                category: "Technology",
                price: invalidPrice,
                stock: 10
            );

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);

            Assert.Equal("price", exception.ParamName);
        }

        [Fact]
        public void Constructor_WhenStockIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            const int invalidStock = -1;

            // Act
            var action = () => new Product(
                code: "PRD-001",
                name: "Mechanical Keyboard",
                category: "Technology",
                price: 50m,
                stock: invalidStock
            );

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);

            Assert.Equal("stock", exception.ParamName);
        }
    }
}