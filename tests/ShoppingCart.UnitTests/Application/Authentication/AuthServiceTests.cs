using ShoppingCart.Application.Authentication;
using ShoppingCart.Domain.Users;

namespace ShoppingCart.UnitTests.Application.Authentication;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUserData()
    {
        // Arrange
        var expiresAtUtc = new DateTime(
            2026,
            7,
            15,
            12,
            0,
            0,
            DateTimeKind.Utc
        );

        var user = new User(
            "customer@shoppingcart.com",
            "stored-password-hash",
            UserRoles.Customer
        );

        var service = new AuthService(
            new FakeUserRepository(user),
            new FakePasswordHasher(isPasswordValid: true),
            new FakeTokenGenerator(
                new GeneratedToken(
                    "generated-jwt-token",
                    expiresAtUtc
                )
            )
        );

        var request = new LoginRequest(
            "customer@shoppingcart.com",
            "Customer123!"
        );

        // Act
        var response = await service.LoginAsync(
            request,
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(response);
        Assert.Equal(
            "generated-jwt-token",
            response.AccessToken
        );
        Assert.Equal(expiresAtUtc, response.ExpiresAtUtc);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(UserRoles.Customer, response.Role);
        Assert.Equal("Bearer", response.TokenType);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var service = new AuthService(
            new FakeUserRepository(null),
            new FakePasswordHasher(isPasswordValid: true),
            new FakeTokenGenerator(
                new GeneratedToken(
                    "unused-token",
                    DateTime.UtcNow
                )
            )
        );

        var request = new LoginRequest(
            "missing@shoppingcart.com",
            "Customer123!"
        );

        // Act
        var response = await service.LoginAsync(
            request,
            CancellationToken.None
        );

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ReturnsNull()
    {
        // Arrange
        var user = new User(
            "customer@shoppingcart.com",
            "stored-password-hash",
            UserRoles.Customer
        );

        var service = new AuthService(
            new FakeUserRepository(user),
            new FakePasswordHasher(isPasswordValid: false),
            new FakeTokenGenerator(
                new GeneratedToken(
                    "unused-token",
                    DateTime.UtcNow
                )
            )
        );

        var request = new LoginRequest(
            "customer@shoppingcart.com",
            "Incorrect123!"
        );

        // Act
        var response = await service.LoginAsync(
            request,
            CancellationToken.None
        );

        // Assert
        Assert.Null(response);
    }

    private sealed class FakeUserRepository(
        User? user)
        : IUserRepository
    {
        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(user);
        }
    }

    private sealed class FakePasswordHasher(
        bool isPasswordValid)
        : IPasswordHasher
    {
        public string Hash(string password)
        {
            return "stored-password-hash";
        }

        public bool Verify(
            string passwordHash,
            string providedPassword)
        {
            return isPasswordValid;
        }
    }

    private sealed class FakeTokenGenerator(
        GeneratedToken generatedToken)
        : ITokenGenerator
    {
        public GeneratedToken Generate(User user)
        {
            return generatedToken;
        }
    }
}