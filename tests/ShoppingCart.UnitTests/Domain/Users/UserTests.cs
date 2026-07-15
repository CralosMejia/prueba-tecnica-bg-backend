using ShoppingCart.Domain.Users;

namespace ShoppingCart.UnitTests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUser()
    {
        // Arrange
        const string email = "Customer@ShoppingCart.com";
        const string passwordHash = "stored-password-hash";

        // Act
        var user = new User(
            email,
            passwordHash,
            UserRoles.Customer
        );

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(
            "customer@shoppingcart.com",
            user.Email
        );
        Assert.Equal(
            passwordHash,
            user.PasswordHash
        );
        Assert.Equal(
            UserRoles.Customer,
            user.Role
        );
    }

    [Theory]
    [InlineData("admin", "Admin")]
    [InlineData(" ADMIN ", "Admin")]
    [InlineData("customer", "Customer")]
    [InlineData(" CUSTOMER ", "Customer")]
    public void Constructor_WithSupportedRole_NormalizesRole(
        string role,
        string expectedRole)
    {
        // Act
        var user = new User(
            "user@shoppingcart.com",
            "stored-password-hash",
            role
        );

        // Assert
        Assert.Equal(expectedRole, user.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyEmail_ThrowsArgumentException(
        string email)
    {
        // Act
        var action = () => new User(
            email,
            "stored-password-hash",
            UserRoles.Customer
        );

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyPasswordHash_ThrowsArgumentException(
        string passwordHash)
    {
        // Act
        var action = () => new User(
            "customer@shoppingcart.com",
            passwordHash,
            UserRoles.Customer
        );

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Supervisor")]
    [InlineData("Manager")]
    public void Constructor_WithUnsupportedRole_ThrowsArgumentException(
        string role)
    {
        // Act
        var action = () => new User(
            "customer@shoppingcart.com",
            "stored-password-hash",
            role
        );

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}