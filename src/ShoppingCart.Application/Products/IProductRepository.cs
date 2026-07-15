using ShoppingCart.Domain.Products;

namespace ShoppingCart.Application.Products;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default
    );

    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}