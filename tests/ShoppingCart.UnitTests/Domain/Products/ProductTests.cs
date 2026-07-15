using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Domain
{
    public class ProductTests
    {


        private static Product CreateProduct(int stock = 10)
        {
            return new Product(
                code: "PRD-001",
                name: "Mechanical Keyboard",
                category: "Technology",
                price: 50m,
                stock: stock
            );
        }
        
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

        [Fact]
        public void DecreaseStock_WhenQuantityIsAvailable_ReducesCurrentStock()
        {
            // Arrange
            var product = new Product(
                code: "PRD-001",
                name: "Mechanical Keyboard",
                category: "Technology",
                price: 50m,
                stock: 10
            );

            // Act
            product.DecreaseStock(quantity: 3);

            // Assert
            Assert.Equal(7, product.Stock);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DecreaseStock_WhenQuantityIsNotPositive_ThrowsArgumentOutOfRangeException(
            int invalidQuantity)
        {
            var product = CreateProduct(stock: 10);

            var action = () => product.DecreaseStock(invalidQuantity);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);

            Assert.Equal("quantity", exception.ParamName);
        }

        [Fact]
        public void DecreaseStock_WhenQuantityExceedsAvailableStock_ThrowsInvalidOperationException()
        {
            var product = CreateProduct(stock: 5);

            var action = () => product.DecreaseStock(quantity: 6);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(5, product.Stock);
        }


        [Fact]
        public void DecreaseStock_WhenQuantityEqualsAvailableStock_LeavesStockAtZero()
        {
            var product = CreateProduct(stock: 5);

            product.DecreaseStock(quantity: 5);

            Assert.Equal(0, product.Stock);
        }
    }
}