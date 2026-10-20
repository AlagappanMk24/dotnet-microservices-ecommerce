using MediatR;
using ProductApi.Application.Features.Products.DTOs;

namespace ProductApi.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string Currency,
    bool IsActive) : IRequest<ProductDto>;