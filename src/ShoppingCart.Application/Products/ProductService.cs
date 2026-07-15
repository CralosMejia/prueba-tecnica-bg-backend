using ShoppingCart.Domain.Products;

namespace ShoppingCart.Application.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(
            search,
            cancellationToken
        );

        return products
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        return product is null
            ? null
            : MapToResponse(product);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Code,
            product.Name,
            product.Category,
            product.Price,
            product.Stock
        );
    }
}