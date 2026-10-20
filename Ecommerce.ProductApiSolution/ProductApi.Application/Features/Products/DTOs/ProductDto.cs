namespace ProductApi.Application.Features.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string Currency,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);