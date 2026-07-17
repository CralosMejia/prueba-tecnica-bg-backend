using Microsoft.EntityFrameworkCore;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Products;
using ShoppingCart.Infrastructure.Persistence;

namespace ShoppingCart.Infrastructure.Products;

public class ProductRepository : IProductRepository
{
    private readonly ShoppingCartDbContext _dbContext;

    public ProductRepository(ShoppingCartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllForAdministrationAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(product =>
                product.Name.Contains(searchTerm) ||
                product.Code.Contains(searchTerm) ||
                product.Category.Contains(searchTerm)
            );
        }

        return await query
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(product =>
                product.Name.Contains(searchTerm) ||
                product.Code.Contains(searchTerm) ||
                product.Category.Contains(searchTerm)
            );
        }

        return await query
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                product => product.Id == id && product.IsActive,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Product>>
    GetByIdsForUpdateAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        if (ids.Count == 0)
        {
            return Array.Empty<Product>();
        }

        var productIds = ids.ToArray();

        return await _dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products
            .FirstOrDefaultAsync(
                product => product.Id == id,
                cancellationToken
            );
    }

    public Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products.AnyAsync(
            product =>
                product.Code == code &&
                (!excludeProductId.HasValue ||
                product.Id != excludeProductId.Value),
            cancellationToken
        );
    }

    public async Task<Product> AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(
            product,
            cancellationToken
        );

        return product;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}