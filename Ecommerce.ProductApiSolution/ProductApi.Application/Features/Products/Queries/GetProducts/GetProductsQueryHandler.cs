using MediatR;
using ProductApi.Application.Common.Interfaces;
using Ecommerce.SharedLibrary.Responses;
using ProductApi.Application.Features.Products.DTOs;

namespace ProductApi.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(
    IProductRepository repository)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync(
            request.Search,
            request.Category,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.Descending,
            cancellationToken);

        var items = result.Items
            .Select(product => new ProductDto(
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
                product.UpdatedAt))
            .ToList();

        return new PagedResult<ProductDto>(
            items,
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }
}