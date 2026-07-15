using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Application.Carts;

namespace ShoppingCart.UnitTests.Api.Controllers;

public sealed class CartControllerTests
{
    [Fact]
    public async Task Get_WithAuthenticatedUser_ReturnsOkAndUsesClaimUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartService = new FakeCartService();
        var controller = CreateController(
            cartService,
            userId
        );

        // Act
        var result = await controller.Get(
            CancellationToken.None
        );

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(
            result.Result
        );

        Assert.Equal(
            StatusCodes.Status200OK,
            okResult.StatusCode
        );

        Assert.Equal(userId, cartService.LastUserId);

        var response = Assert.IsType<CartResponse>(
            okResult.Value
        );

        Assert.Empty(response.Items);
    }

    [Fact]
    public async Task AddItem_WithAuthenticatedUser_ReturnsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var cartService = new FakeCartService();
        var controller = CreateController(
            cartService,
            userId
        );

        var request = new AddCartItemRequest(
            productId,
            Quantity: 2
        );

        // Act
        var result = await controller.AddItem(
            request,
            CancellationToken.None
        );

        // Assert
        var createdResult =
            Assert.IsType<CreatedAtActionResult>(
                result.Result
            );

        Assert.Equal(
            StatusCodes.Status201Created,
            createdResult.StatusCode
        );

        Assert.Equal(
            nameof(CartController.Get),
            createdResult.ActionName
        );

        Assert.Equal(userId, cartService.LastUserId);
        Assert.Equal(productId, cartService.LastProductId);
    }

    [Fact]
    public async Task Get_WithoutValidUserIdClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var cartService = new FakeCartService();

        var controller = new CartController(
            cartService
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        // Act
        var action = () => controller.Get(
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            UnauthorizedAccessException
        >(action);
    }

    private static CartController CreateController(
        ICartService cartService,
        Guid userId)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString()
            )
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test"
        );

        var principal = new ClaimsPrincipal(identity);

        return new CartController(cartService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };
    }

    private sealed class FakeCartService : ICartService
    {
        public Guid? LastUserId { get; private set; }

        public Guid? LastProductId { get; private set; }

        public Task<CartResponse> GetAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;

            return Task.FromResult(
                CartResponse.Empty
            );
        }

        public Task<CartResponse> AddItemAsync(
            Guid userId,
            AddCartItemRequest request,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            LastProductId = request.ProductId;

            return Task.FromResult(
                CartResponse.Empty
            );
        }

        public Task<CartResponse> UpdateQuantityAsync(
            Guid userId,
            Guid productId,
            UpdateCartItemRequest request,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            LastProductId = productId;

            return Task.FromResult(
                CartResponse.Empty
            );
        }

        public Task<CartResponse> RemoveItemAsync(
            Guid userId,
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            LastProductId = productId;

            return Task.FromResult(
                CartResponse.Empty
            );
        }

        public Task<CartResponse> ClearAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;

            return Task.FromResult(
                CartResponse.Empty
            );
        }
    }
}