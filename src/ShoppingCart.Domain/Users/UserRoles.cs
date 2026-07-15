namespace ShoppingCart.Domain.Users;

public static class UserRoles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";

    public static string Normalize(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return role.Trim().ToLowerInvariant() switch
        {
            "customer" => Customer,
            "admin" => Admin,
            _ => throw new ArgumentException(
                $"Role '{role}' is not supported.",
                nameof(role)
            )
        };
    }
}