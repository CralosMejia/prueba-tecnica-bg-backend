namespace ShoppingCart.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ProductResponse>> GetAllForAdministrationAsync(
        string? search,
        CancellationToken cancellationToken = default
    );


    Task<ProductResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ProductResponse> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default
    );

    Task ToggleActivityStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}