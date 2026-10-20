namespace ProductApi.Presentation.Models;

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string Currency,
    bool IsActive);