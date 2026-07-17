using ShoppingCart.Domain.Products;

namespace ShoppingCart.Application.Products;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Product>> GetAllForAdministrationAsync(
        string? search,
        CancellationToken cancellationToken = default
    );

    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );

    Task<Product?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default
    );

    Task<Product> AddAsync(
        Product product,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}