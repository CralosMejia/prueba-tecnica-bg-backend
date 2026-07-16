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

    public async Task<IReadOnlyList<Product>> GetAllAsync(
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

    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                product => product.Id == id,
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
}