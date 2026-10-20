using MediatR;
using ProductApi.Application.Common.Interfaces;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository repository)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(
            request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        product.SoftDelete();

        await repository.SaveChangesAsync(cancellationToken);
    }
}