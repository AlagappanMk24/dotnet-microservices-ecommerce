using MediatR;
using OrderApi.Application.Features.Orders.DTOs;
namespace OrderApi.Application.Features.Orders.Queries.GetOrderById;
public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;