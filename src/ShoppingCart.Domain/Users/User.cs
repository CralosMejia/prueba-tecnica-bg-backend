namespace ShoppingCart.Domain.Users;

public sealed class User
{
    // Constructor requerido por Entity Framework Core
    private User()
    {
    }

    public User(
        string email,
        string passwordHash,
        string role)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        PasswordHash = ValidatePasswordHash(passwordHash);
        Role = UserRoles.Normalize(role);
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } =
        string.Empty;

    public string PasswordHash { get; private set; } =
        string.Empty;

    public string Role { get; private set; } =
        string.Empty;

    public static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return email
            .Trim()
            .ToLowerInvariant();
    }

    private static string ValidatePasswordHash(
        string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            passwordHash
        );

        return passwordHash.Trim();
    }
}