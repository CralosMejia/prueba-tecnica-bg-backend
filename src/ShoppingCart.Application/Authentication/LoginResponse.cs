namespace ShoppingCart.Application.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Email,
    string Role,
    string TokenType = "Bearer"
);