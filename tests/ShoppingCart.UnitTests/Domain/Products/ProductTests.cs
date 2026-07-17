using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Domain
{
    public class ProductTests
    {


        private static Product CreateProduct(string name = "Mechanical Keyboard", string category = "Technology", decimal price = 50m, int stock = 10)
        {
            return new Product(
                code: "PRD-001",
                name: name,
                category: category,
                price: price,
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

        [Fact]
        public void UpdateProduct_WhenValidParameters_UpdatesProductProperties()
        {
            // Arrange
            var product = CreateProduct(stock: 10);
            var newName = "Updated Keyboard";
            var newCategory = "Updated Technology";
            var newPrice = 60m;
            var newStock = 15;

            // Act
            product.UpdateProduct(newName, newCategory, newPrice, newStock);

            // Assert
            Assert.Equal(newName, product.Name);
            Assert.Equal(newCategory, product.Category);
            Assert.Equal(newPrice, product.Price);
            Assert.Equal(newStock, product.Stock);
        }

        [Fact]
        public void UpdateProduct_WhenNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var product = CreateProduct(stock: 10);
            var newName = "";
            var newCategory = "Updated Technology";
            var newPrice = 60m;
            var newStock = 15;

            // Act
            var action = () => product.UpdateProduct(newName, newCategory, newPrice, newStock);

            // Assert
            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void UpdateProduct_WhenCategoryIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var product = CreateProduct(stock: 10);
            var newName = "Updated Keyboard";
            var newCategory = "";
            var newPrice = 60m;
            var newStock = 15;

            // Act
            var action = () => product.UpdateProduct(newName, newCategory, newPrice, newStock);

            // Assert
            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void UpdateProduct_WhenPriceIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var product = CreateProduct(stock: 10);
            var newName = "Updated Keyboard";
            var newCategory = "Updated Technology";
            var newPrice = -60m;
            var newStock = 15;

            // Act
            var action = () => product.UpdateProduct(newName, newCategory, newPrice, newStock);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);
            Assert.Equal("price", exception.ParamName);
        }

        [Fact]
        public void UpdateProduct_WhenStockIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var product = CreateProduct(stock: 10);
            var newName = "Updated Keyboard";
            var newCategory = "Updated Technology";
            var newPrice = 60m;
            var newStock = -15;

            // Act
            var action = () => product.UpdateProduct(newName, newCategory, newPrice, newStock);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);
            Assert.Equal("stock", exception.ParamName);
        }

        [Fact]
        public void ChangeActivityStatus_TogglesIsActive()
        {
            // Arrange
            var product = CreateProduct();

            // Act
            product.ChangeActivityStatus();

            // Assert
            Assert.False(product.IsActive);
        }
        [Fact]
        public void ChangeActivityStatus_CalledTwice_RestoresOriginalStatus()
        {
            var product = CreateProduct();

            product.ChangeActivityStatus();
            Assert.False(product.IsActive);

            product.ChangeActivityStatus();
            Assert.True(product.IsActive);
        }

        [Fact]
        public void NewProduct_isactiveByDefault()
        {
            // Arrange
            var product = CreateProduct();

            // Act & Assert
            Assert.True(product.IsActive);
        }

    }
}