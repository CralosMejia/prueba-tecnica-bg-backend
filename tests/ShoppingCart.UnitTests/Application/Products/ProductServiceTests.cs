using ShoppingCart.Application.Common.Exceptions;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Application.Products;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsMappedProducts()
    {
        // Arrange
        var products = new[]
        {
            CreateProduct(
                code: "PRD-001",
                name: "Mechanical Keyboard",
                price: 80m,
                stock: 10
            ),
            CreateProduct(
                code: "PRD-002",
                name: "Wireless Mouse",
                price: 35m,
                stock: 20
            )
        };

        var repository =
            new FakeProductRepository(products);

        var service =
            new ProductService(repository);

        // Act
        var result = await service.GetAllAsync(
            search: null,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(products[0].Id, result[0].Id);
        Assert.Equal(products[0].Code, result[0].Code);
        Assert.Equal(products[0].Name, result[0].Name);
        Assert.Equal(
            products[0].Category,
            result[0].Category
        );
        Assert.Equal(products[0].Price, result[0].Price);
        Assert.Equal(products[0].Stock, result[0].Stock);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository =
            new FakeProductRepository();

        var service =
            new ProductService(repository);

        // Act
        var result = await service.GetByIdAsync(
            Guid.NewGuid(),
            CancellationToken.None
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesActiveProduct()
    {
        // Arrange
        var repository =
            new FakeProductRepository();

        var service =
            new ProductService(repository);

        var request = new CreateProductRequest
        {
            Code = "PRD-003",
            Name = "Gaming Monitor",
            Category = "Technology",
            Price = 300m,
            Stock = 15
        };

        // Act
        var result = await service.CreateAsync(
            request,
            CancellationToken.None
        );

        // Assert
        var createdProduct =
            Assert.Single(repository.Products);

        Assert.Equal(request.Code, createdProduct.Code);
        Assert.Equal(request.Name, createdProduct.Name);
        Assert.Equal(
            request.Category,
            createdProduct.Category
        );
        Assert.Equal(request.Price, createdProduct.Price);
        Assert.Equal(request.Stock, createdProduct.Stock);
        Assert.True(createdProduct.IsActive);

        Assert.Equal(createdProduct.Id, result.Id);
        Assert.Equal(request.Code, result.Code);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Category, result.Category);
        Assert.Equal(request.Price, result.Price);
        Assert.Equal(request.Stock, result.Stock);

        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeAlreadyExists_ThrowsBusinessConflictException()
    {
        // Arrange
        var existingProduct = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10
        );

        var repository =
            new FakeProductRepository(existingProduct);

        var service =
            new ProductService(repository);

        var request = new CreateProductRequest
        {
            Code = "prd-001",
            Name = "New Product",
            Category = "Technology",
            Price = 100m,
            Stock = 5
        };

        // Act
        var action = () => service.CreateAsync(
            request,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            BusinessConflictException
        >(action);

        Assert.Single(repository.Products);
        Assert.Equal(0, repository.AddCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_WithOnlyPrice_UpdatesOnlyPrice()
    {
        // Arrange
        var product = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10
        );

        var repository =
            new FakeProductRepository(product);

        var service =
            new ProductService(repository);

        var request = new UpdateProductRequest
        {
            Price = 95m
        };

        // Act
        var response = await service.UpdateAsync(
            product.Id,
            request,
            CancellationToken.None
        );

        // Assert
        Assert.Equal("PRD-001", product.Code);
        Assert.Equal("Mechanical Keyboard", product.Name);
        Assert.Equal("Technology", product.Category);
        Assert.Equal(95m, product.Price);
        Assert.Equal(10, product.Stock);
        Assert.True(product.IsActive);

        Assert.Equal(95m, response.Price);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_WithNameAndStock_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var product = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10
        );

        var repository =
            new FakeProductRepository(product);

        var service =
            new ProductService(repository);

        var request = new UpdateProductRequest
        {
            Name = "Updated Keyboard",
            Stock = 20
        };

        // Act
        var response = await service.UpdateAsync(
            product.Id,
            request,
            CancellationToken.None
        );

        // Assert
        Assert.Equal("PRD-001", product.Code);
        Assert.Equal("Updated Keyboard", product.Name);
        Assert.Equal("Technology", product.Category);
        Assert.Equal(80m, product.Price);
        Assert.Equal(20, product.Stock);
        Assert.True(product.IsActive);

        Assert.Equal("Updated Keyboard", response.Name);
        Assert.Equal(20, response.Stock);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyRequest_ThrowsArgumentException()
    {
        // Arrange
        var product = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10
        );

        var repository =
            new FakeProductRepository(product);

        var service =
            new ProductService(repository);

        var request = new UpdateProductRequest();

        // Act
        var action = () => service.UpdateAsync(
            product.Id,
            request,
            CancellationToken.None
        );

        // Assert
        var exception = await Assert.ThrowsAsync<
            ArgumentException
        >(action);

        Assert.Equal("request", exception.ParamName);
        Assert.Equal(0, repository.SaveChangesCalls);

        Assert.Equal("Mechanical Keyboard", product.Name);
        Assert.Equal("Technology", product.Category);
        Assert.Equal(80m, product.Price);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsResourceNotFoundException()
    {
        // Arrange
        var repository =
            new FakeProductRepository();

        var service =
            new ProductService(repository);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Category = "Technology",
            Price = 90m,
            Stock = 15
        };

        // Act
        var action = () => service.UpdateAsync(
            Guid.NewGuid(),
            request,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            ResourceNotFoundException
        >(action);

        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ToggleActivityStatusAsync_WhenProductIsActive_DeactivatesProduct()
    {
        // Arrange
        var existingProduct = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10,
            isActive: true
        );

        var repository =
            new FakeProductRepository(existingProduct);

        var service =
            new ProductService(repository);

        // Act
        await service.ToggleActivityStatusAsync(
            existingProduct.Id,
            CancellationToken.None
        );

        // Assert
        Assert.False(existingProduct.IsActive);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ToggleActivityStatusAsync_WhenProductIsInactive_ActivatesProduct()
    {
        // Arrange
        var existingProduct = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10
        );
        existingProduct.ChangeActivityStatus();
        Assert.False(existingProduct.IsActive);

        var repository =
            new FakeProductRepository(existingProduct);

        var service =
            new ProductService(repository);

        // Act
        await service.ToggleActivityStatusAsync(
            existingProduct.Id,
            CancellationToken.None
        );

        // Assert
        Assert.True(existingProduct.IsActive);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ToggleActivityStatusAsync_CalledTwice_RestoresOriginalStatus()
    {
        // Arrange
        var existingProduct = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10,
            isActive: true
        );

        var repository =
            new FakeProductRepository(existingProduct);

        var service =
            new ProductService(repository);

        // Act
        await service.ToggleActivityStatusAsync(
            existingProduct.Id,
            CancellationToken.None
        );

        await service.ToggleActivityStatusAsync(
            existingProduct.Id,
            CancellationToken.None
        );

        // Assert
        Assert.True(existingProduct.IsActive);
        Assert.Equal(2, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ToggleActivityStatusAsync_WhenProductDoesNotExist_ThrowsResourceNotFoundException()
    {
        // Arrange
        var repository =
            new FakeProductRepository();

        var service =
            new ProductService(repository);

        // Act
        var action = () =>
            service.ToggleActivityStatusAsync(
                Guid.NewGuid(),
                CancellationToken.None
            );

        // Assert
        await Assert.ThrowsAsync<
            ResourceNotFoundException
        >(action);

        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task GetAllForAdministrationAsync_ReturnsActiveAndInactiveProducts()
    {
        // Arrange
        var activeProduct = CreateProduct(
            code: "PRD-001",
            name: "Mechanical Keyboard",
            price: 80m,
            stock: 10
        );

        var inactiveProduct = CreateProduct(
            code: "PRD-002",
            name: "Wireless Mouse",
            price: 35m,
            stock: 20
        );

        inactiveProduct.ChangeActivityStatus();

        var repository =
            new FakeProductRepository(
                activeProduct,
                inactiveProduct
            );

        var service =
            new ProductService(repository);

        // Act
        var result =
            await service.GetAllForAdministrationAsync(
                search: null,
                CancellationToken.None
            );

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            product =>
                product.Id == activeProduct.Id &&
                product.IsActive
        );

        Assert.Contains(
            result,
            product =>
                product.Id == inactiveProduct.Id &&
                !product.IsActive
        );
    }

    private static Product CreateProduct(
        string code,
        string name,
        decimal price,
        int stock,
        string category = "Technology",
        bool isActive = true)
    {
        return new Product(
            code,
            name,
            category,
            price,
            stock
        );
    }

    private sealed class FakeProductRepository
        : IProductRepository
    {
        private readonly List<Product> _products;

        public FakeProductRepository(
            params Product[] products)
        {
            _products = products.ToList();
        }

        public IReadOnlyCollection<Product> Products =>
            _products.AsReadOnly();

        public int AddCalls { get; private set; }

        public int SaveChangesCalls { get; private set; }

        public Task<IReadOnlyList<Product>> GetAllAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<Product> query =
                _products.Where(product =>
                    product.IsActive
                );

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(product =>
                    product.Code.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    product.Name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    product.Category.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            IReadOnlyList<Product> result =
                query.ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Product>>
            GetAllForAdministrationAsync(
                string? search,
                CancellationToken cancellationToken = default)
        {
            IEnumerable<Product> query = _products;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(product =>
                    product.Code.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    product.Name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    product.Category.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            IReadOnlyList<Product> result = query.ToList();

            return Task.FromResult(result);
        }

        public Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var product = _products.FirstOrDefault(
                currentProduct =>
                    currentProduct.Id == id &&
                    currentProduct.IsActive
            );

            return Task.FromResult(product);
        }

        public Task<Product?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var product = _products.FirstOrDefault(
                currentProduct =>
                    currentProduct.Id == id
            );

            return Task.FromResult(product);
        }

        public Task<IReadOnlyList<Product>>
            GetByIdsForUpdateAsync(
                IReadOnlyCollection<Guid> ids,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> result =
                _products
                    .Where(product =>
                        ids.Contains(product.Id)
                    )
                    .ToList();

            return Task.FromResult(result);
        }

        public Task<bool> ExistsByCodeAsync(
            string code,
            Guid? excludedProductId = null,
            CancellationToken cancellationToken = default)
        {
            var exists = _products.Any(product =>
                string.Equals(
                    product.Code,
                    code,
                    StringComparison.OrdinalIgnoreCase
                ) &&
                product.Id != excludedProductId
            );

            return Task.FromResult(exists);
        }

        public Task<Product> AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            _products.Add(product);
            AddCalls++;

            return Task.FromResult(product);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;

            return Task.CompletedTask;
        }
    }
}