using Ecommerce.SharedLibrary.Responses;
using MediatR;
using OrderApi.Application.Features.Orders.DTOs;

namespace OrderApi.Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<OrderDto>>;