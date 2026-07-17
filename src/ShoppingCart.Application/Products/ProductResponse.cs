namespace ShoppingCart.Application.Products;

public record ProductResponse(
    Guid Id,
    string Code,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    bool IsActive
);