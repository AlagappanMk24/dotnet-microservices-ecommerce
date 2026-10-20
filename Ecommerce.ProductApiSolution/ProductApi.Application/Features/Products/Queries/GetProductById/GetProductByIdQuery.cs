using MediatR;
using ProductApi.Application.Features.Products.DTOs;

namespace ProductApi.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;