using MediatR;
using Ecommerce.SharedLibrary.Responses;
using ProductApi.Application.Features.Products.DTOs;

namespace ProductApi.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    string? Search,
    string? Category,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "createdAt",
    bool Descending = true)
    : IRequest<PagedResult<ProductDto>>;