using ShoppingCart.Domain.Products;
using ShoppingCart.Application.Common.Exceptions;

namespace ShoppingCart.Application.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }


    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if(request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var codeExists =
        await _productRepository.ExistsByCodeAsync(
            request.Code,
            excludeProductId: null,
            cancellationToken
        );

        if (codeExists)
        {
            throw new BusinessConflictException($"Product with code '{request.Code}' already exists.");
        }

        var product = new Product(
            request.Code,
            request.Name,
            request.Category,
            request.Price,
            request.Stock
        );

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if(id == Guid.Empty)
        {
            throw new ArgumentException("Product ID must be provided.", nameof(id));
        }
        if(request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        if (
            request.Name is null &&
            request.Category is null &&
            request.Price is null &&
            request.Stock is null
        )
        {
            throw new ArgumentException(
                "At least one field must be provided.",
                nameof(request)
            );
        }

        var product = await _productRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            throw new ResourceNotFoundException($"Product with ID '{id}' not found.", nameof(id));
        }

        product.UpdateProduct(
            request.Name,
            request.Category,
            request.Price,
            request.Stock
        );

        await _productRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    public async Task ToggleActivityStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Product ID must be provided.", nameof(id));
        }

        var product = await _productRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            throw new ResourceNotFoundException($"Product with ID '{id}' not found.", nameof(id));
        }

        product.ChangeActivityStatus();

        await _productRepository.SaveChangesAsync(cancellationToken);
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

    public async Task<IReadOnlyList<ProductResponse>> GetAllForAdministrationAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllForAdministrationAsync(
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
            product.Stock,
            product.IsActive
        );
    }
}