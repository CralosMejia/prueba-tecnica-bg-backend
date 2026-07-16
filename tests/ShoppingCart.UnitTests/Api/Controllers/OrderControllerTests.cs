using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Application.Orders;

namespace ShoppingCart.UnitTests.Api.Controllers;

public sealed class OrderControllerTests
{
    [Fact]
    public async Task Checkout_WithAuthenticatedUser_ReturnsCreatedOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var orderResponse = CreateOrderResponse(orderId);
        var orderService = new FakeOrderService
        {
            CheckoutResponse = orderResponse
        };

        var controller = CreateController(
            orderService,
            userId
        );

        // Act
        var result = await controller.Checkout(
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
            nameof(OrderController.GetById),
            createdResult.ActionName
        );

        Assert.Equal(
            orderId,
            createdResult.RouteValues?["id"]
        );

        Assert.Equal(userId, orderService.LastUserId);

        var response = Assert.IsType<OrderResponse>(
            createdResult.Value
        );

        Assert.Equal(orderId, response.Id);
    }

    [Fact]
    public async Task GetAll_WithAuthenticatedUser_ReturnsUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var orders = new[]
        {
            CreateOrderResponse(Guid.NewGuid()),
            CreateOrderResponse(Guid.NewGuid())
        };

        var orderService = new FakeOrderService
        {
            OrdersResponse = orders
        };

        var controller = CreateController(
            orderService,
            userId
        );

        // Act
        var result = await controller.GetAll(
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

        Assert.Equal(userId, orderService.LastUserId);

        var response = Assert.IsAssignableFrom<
            IReadOnlyList<OrderResponse>
        >(okResult.Value);

        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task GetById_WithAuthenticatedUser_ReturnsOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var orderService = new FakeOrderService
        {
            GetByIdResponse =
                CreateOrderResponse(orderId)
        };

        var controller = CreateController(
            orderService,
            userId
        );

        // Act
        var result = await controller.GetById(
            orderId,
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

        Assert.Equal(userId, orderService.LastUserId);
        Assert.Equal(orderId, orderService.LastOrderId);

        var response = Assert.IsType<OrderResponse>(
            okResult.Value
        );

        Assert.Equal(orderId, response.Id);
    }

    private static OrderController CreateController(
        IOrderService orderService,
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

        return new OrderController(orderService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static OrderResponse CreateOrderResponse(
        Guid orderId)
    {
        return new OrderResponse(
            Id: orderId,
            CreatedAtUtc: DateTime.UtcNow,
            Items: Array.Empty<OrderItemResponse>(),
            Subtotal: 0m,
            Discount: 0m,
            Total: 0m
        );
    }

    private sealed class FakeOrderService
        : IOrderService
    {
        public Guid? LastUserId { get; private set; }

        public Guid? LastOrderId { get; private set; }

        public OrderResponse? CheckoutResponse { get; init; }

        public IReadOnlyList<OrderResponse> OrdersResponse
        {
            get;
            init;
        } = Array.Empty<OrderResponse>();

        public OrderResponse? GetByIdResponse { get; init; }

        public Task<OrderResponse> CheckoutAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;

            return Task.FromResult(
                CheckoutResponse
                ?? throw new InvalidOperationException(
                    "Checkout response was not configured."
                )
            );
        }

        public Task<IReadOnlyList<OrderResponse>> GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;

            return Task.FromResult(OrdersResponse);
        }

        public Task<OrderResponse> GetByIdAsync(
            Guid userId,
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            LastOrderId = orderId;

            return Task.FromResult(
                GetByIdResponse
                ?? throw new InvalidOperationException(
                    "Order response was not configured."
                )
            );
        }
    }
}