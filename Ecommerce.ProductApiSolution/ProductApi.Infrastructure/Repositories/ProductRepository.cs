using Microsoft.EntityFrameworkCore;
using ProductApi.Application.Common.Interfaces;
using Ecommerce.SharedLibrary.Responses;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Persistence;

namespace ProductApi.Infrastructure.Repositories;

public sealed class ProductRepository(ProductDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id,CancellationToken cancellationToken)
    {
        return dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public Task<bool> ExistsBySkuAsync(string sku, Guid? excludeProductId,CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking()
                        .Where(product => product.Sku == sku);

        if (excludeProductId.HasValue)
        {
            query = query.Where(product => product.Id != excludeProductId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        string? search,
        string? category,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(product =>
                product.Name.Contains(term) ||
                product.Sku.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var selectedCategory = category.Trim();

            query = query.Where(product =>
                product.Category == selectedCategory);
        }

        if (isActive.HasValue)
        {
            query = query.Where(product =>
                product.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, descending);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, pageNumber, pageSize, totalCount);
    }

    public void Remove(Product product)
    {
        product.SoftDelete();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string sortBy,
        bool descending)
    {
        return (sortBy.ToLowerInvariant(), descending) switch
        {
            ("name", false) => query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id),

            ("name", true) => query
                .OrderByDescending(product => product.Name)
                .ThenBy(product => product.Id),

            ("sku", false) => query
                .OrderBy(product => product.Sku)
                .ThenBy(product => product.Id),

            ("sku", true) => query
                .OrderByDescending(product => product.Sku)
                .ThenBy(product => product.Id),

            ("price", false) => query
                .OrderBy(product => product.Price)
                .ThenBy(product => product.Id),

            ("price", true) => query
                .OrderByDescending(product => product.Price)
                .ThenBy(product => product.Id),

            ("createdat", false) => query
                .OrderBy(product => product.CreatedAt)
                .ThenBy(product => product.Id),

            _ => query
                .OrderByDescending(product => product.CreatedAt)
                .ThenBy(product => product.Id)
        };
    }
    public void Update(Product product)
    {
        dbContext.Products.Update(product);
    }
}