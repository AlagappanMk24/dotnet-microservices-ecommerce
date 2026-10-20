using Ecommerce.SharedLibrary.Events;
using MediatR;
using ProductApi.Application.Common.Interfaces;
using ProductApi.Application.Features.Products.DTOs;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository repository, IProductEventPublisher productEventPublisher)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var sku = request.Sku.Trim();

        if (await repository.ExistsBySkuAsync(
                sku, null, cancellationToken))
        {
            throw new DuplicateProductSkuException(sku);
        }

        var product = Product.Create(
            sku,
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.StockQuantity,
            request.Currency);

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var productCreatedEvent = new ProductCreatedEvent(
         product.Id,
         product.Sku,
         product.Name);

        await productEventPublisher.PublishProductCreatedAsync(
            productCreatedEvent,
            cancellationToken);

        return Map(product);
    }

    private static ProductDto Map(Product product) =>
        new(
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