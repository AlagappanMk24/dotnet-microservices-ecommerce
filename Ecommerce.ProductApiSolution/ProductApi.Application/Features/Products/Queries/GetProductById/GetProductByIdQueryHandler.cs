using MediatR;
using ProductApi.Application.Common.Interfaces;
using ProductApi.Application.Features.Products.DTOs;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(
    IProductRepository repository)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(
            request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        return new ProductDto(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.Category,
            product.Price,
            product.StockQuantity,
            product.Currency,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);
    }
}