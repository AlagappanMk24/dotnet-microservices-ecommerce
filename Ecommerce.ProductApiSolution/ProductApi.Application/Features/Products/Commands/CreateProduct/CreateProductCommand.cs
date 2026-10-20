using MediatR;
using ProductApi.Application.Features.Products.DTOs;

namespace ProductApi.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string Currency) : IRequest<ProductDto>;