using MediatR;
using ProductApi.Application.Common.Interfaces;
using ProductApi.Application.Features.Products.DTOs;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository repository)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(
            request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        product.UpdateDetails(
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.StockQuantity,
            request.Currency,
            request.IsActive);

        await repository.SaveChangesAsync(cancellationToken);

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