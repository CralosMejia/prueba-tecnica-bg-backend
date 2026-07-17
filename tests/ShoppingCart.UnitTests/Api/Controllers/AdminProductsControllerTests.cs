using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Application.Products;

namespace ShoppingCart.UnitTests.Api.Controllers;

public sealed class AdminProductsControllerTests
{
    [Fact]
    public async Task Create_WithValidRequest_Returns201Created()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var expectedResponse = new ProductResponse(
            productId,
            "PRD-003",
            "Gaming Monitor",
            "Technology",
            300m,
            15,
            true
        );

        var service = new FakeProductService
        {
            CreateResponse = expectedResponse
        };

        var controller =
            new AdminProductsController(service);

        var request = new CreateProductRequest
        {
            Code = "PRD-003",
            Name = "Gaming Monitor",
            Category = "Technology",
            Price = 300m,
            Stock = 15
        };

        // Act
        var result = await controller.Create(
            request,
            CancellationToken.None
        );

        // Assert
        var createdResult =
            Assert.IsType<CreatedResult>(
                result.Result
            );

        Assert.Equal(
            StatusCodes.Status201Created,
            createdResult.StatusCode
        );

        Assert.Equal(
            $"/api/products/{productId}",
            createdResult.Location
        );

        var response =
            Assert.IsType<ProductResponse>(
                createdResult.Value
            );

        Assert.Equal(expectedResponse, response);
        Assert.Equal(1, service.CreateCalls);
        Assert.Same(request, service.LastCreateRequest);
    }

    [Fact]
    public async Task Update_WithValidRequest_Returns200Ok()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var expectedResponse = new ProductResponse(
            productId,
            "PRD-001",
            "Updated Mechanical Keyboard",
            "Computer Accessories",
            90m,
            15,
            true
        );

        var service = new FakeProductService
        {
            UpdateResponse = expectedResponse
        };

        var controller =
            new AdminProductsController(service);

        var request = new UpdateProductRequest
        {
            Price = 90m
        };

        // Act
        var result = await controller.Update(
            productId,
            request,
            CancellationToken.None
        );

        // Assert
        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result
            );

        Assert.Equal(
            StatusCodes.Status200OK,
            okResult.StatusCode
        );

        var response =
            Assert.IsType<ProductResponse>(
                okResult.Value
            );

        Assert.Equal(expectedResponse, response);
        Assert.Equal(1, service.UpdateCalls);
        Assert.Equal(productId, service.LastUpdatedProductId);
        Assert.Same(request, service.LastUpdateRequest);
    }

    [Fact]
    public async Task ToggleStatus_WithExistingProduct_Returns204NoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var service = new FakeProductService();

        var controller =
            new AdminProductsController(service);

        // Act
        var result = await controller.ToggleStatus(
            productId,
            CancellationToken.None
        );

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, service.ToggleStatusCalls);
        Assert.Equal(
            productId,
            service.LastToggledProductId
        );
    }

    [Fact]
    public async Task GetAll_Returns200WithActiveAndInactiveProducts()
    {
        // Arrange
        IReadOnlyList<ProductResponse> expectedProducts =
        [
            new(
                Guid.NewGuid(),
                "PRD-001",
                "Mechanical Keyboard",
                "Technology",
                80m,
                10,
                true
            ),
            new(
                Guid.NewGuid(),
                "PRD-002",
                "Wireless Mouse",
                "Technology",
                35m,
                20,
                false
            )
        ];

        var service = new FakeProductService
        {
            AdministrativeProductsResponse =
                expectedProducts
        };

        var controller =
            new AdminProductsController(service);

        // Act
        var result = await controller.GetAll(
            search: null,
            CancellationToken.None
        );

        // Assert
        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result
            );

        var products =
            Assert.IsAssignableFrom<
                IReadOnlyList<ProductResponse>
            >(okResult.Value);

        Assert.Equal(2, products.Count);
        Assert.Contains(
            products,
            product => product.IsActive
        );
        Assert.Contains(
            products,
            product => !product.IsActive
        );

        Assert.Equal(
            1,
            service.AdministrativeGetAllCalls
        );
    }

    private sealed class FakeProductService
        : IProductService
    {
        public IReadOnlyList<ProductResponse>?
            AdministrativeProductsResponse
        {
            get;
            init;
        }

        public ProductResponse? CreateResponse
        {
            get;
            init;
        }

        public ProductResponse? UpdateResponse
        {
            get;
            init;
        }

        public int AdministrativeGetAllCalls
        {
            get;
            private set;
        }

        public int CreateCalls
        {
            get;
            private set;
        }

        public int UpdateCalls
        {
            get;
            private set;
        }

        public int ToggleStatusCalls
        {
            get;
            private set;
        }

        public CreateProductRequest? LastCreateRequest
        {
            get;
            private set;
        }

        public UpdateProductRequest? LastUpdateRequest
        {
            get;
            private set;
        }

        public Guid? LastUpdatedProductId
        {
            get;
            private set;
        }

        public Guid? LastToggledProductId
        {
            get;
            private set;
        }

        public Task<ProductResponse> CreateAsync(
            CreateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            LastCreateRequest = request;

            return Task.FromResult(
                CreateResponse
                ?? throw new InvalidOperationException(
                    "CreateResponse was not configured."
                )
            );
        }

        public Task<ProductResponse> UpdateAsync(
            Guid id,
            UpdateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            LastUpdatedProductId = id;
            LastUpdateRequest = request;

            return Task.FromResult(
                UpdateResponse
                ?? throw new InvalidOperationException(
                    "UpdateResponse was not configured."
                )
            );
        }

        public Task ToggleActivityStatusAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            ToggleStatusCalls++;
            LastToggledProductId = id;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ProductResponse>> GetAllAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Administrative controller tests do not use public product queries."
            );
        }

        public Task<IReadOnlyList<ProductResponse>>
            GetAllForAdministrationAsync(
                string? search,
                CancellationToken cancellationToken = default)
        {
            AdministrativeGetAllCalls++;

            IReadOnlyList<ProductResponse> result =
                AdministrativeProductsResponse
                ?? [];

            return Task.FromResult(result);
        }

        public Task<ProductResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Administrative controller tests do not use product detail queries."
            );
        }
    }
}