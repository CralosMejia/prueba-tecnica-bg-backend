using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Application.Authentication;
using ShoppingCart.Domain.Users;

namespace ShoppingCart.UnitTests.Api.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var expiresAtUtc = new DateTime(
            2026,
            7,
            16,
            12,
            0,
            0,
            DateTimeKind.Utc
        );

        var loginResponse = new LoginResponse(
            AccessToken: "generated-token",
            ExpiresAtUtc: expiresAtUtc,
            Email: "customer@shoppingcart.com",
            Role: UserRoles.Customer
        );

        var authService = new FakeAuthService(loginResponse);
        var controller = new AuthController(authService);

        var request = new LoginRequest(
            "customer@shoppingcart.com",
            "Customer123!"
        );

        // Act
        var result = await controller.Login(
            request,
            CancellationToken.None
        );

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(
            result.Result
        );

        var response = Assert.IsType<LoginResponse>(
            okResult.Value
        );

        Assert.Equal(
            StatusCodes.Status200OK,
            okResult.StatusCode
        );

        Assert.Equal(
            "generated-token",
            response.AccessToken
        );

        Assert.Equal(
            "customer@shoppingcart.com",
            response.Email
        );

        Assert.Equal(
            UserRoles.Customer,
            response.Role
        );
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedProblemDetails()
    {
        // Arrange
        var authService = new FakeAuthService(null);
        var controller = new AuthController(authService);

        var request = new LoginRequest(
            "customer@shoppingcart.com",
            "Incorrect123!"
        );

        // Act
        var result = await controller.Login(
            request,
            CancellationToken.None
        );

        // Assert
        var unauthorizedResult =
            Assert.IsType<UnauthorizedObjectResult>(
                result.Result
            );

        var problemDetails =
            Assert.IsType<ProblemDetails>(
                unauthorizedResult.Value
            );

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            unauthorizedResult.StatusCode
        );

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            problemDetails.Status
        );

        Assert.Equal(
            "Credenciales inválidas",
            problemDetails.Title
        );

        Assert.Equal(
            "El correo o la contraseña son incorrectos.",
            problemDetails.Detail
        );
    }

    private sealed class FakeAuthService(
        LoginResponse? response)
        : IAuthService
    {
        public Task<LoginResponse?> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }
}