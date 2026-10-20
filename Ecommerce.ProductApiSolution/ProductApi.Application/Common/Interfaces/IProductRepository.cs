using Ecommerce.SharedLibrary.Repositories;
using Ecommerce.SharedLibrary.Responses;
using ProductApi.Domain.Entities;

namespace ProductApi.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> ExistsBySkuAsync(
        string sku,
        Guid? excludeProductId,
        CancellationToken cancellationToken);

    Task<PagedResult<Product>> GetPagedAsync(
        string? search,
        string? category,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}