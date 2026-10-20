namespace OrderApi.Application.Common.Interfaces;

public interface IProductCatalogClient
{
    Task<ProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken);
}

public sealed record ProductSnapshot(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    string Currency,
    bool IsActive,
    int StockQuantity);