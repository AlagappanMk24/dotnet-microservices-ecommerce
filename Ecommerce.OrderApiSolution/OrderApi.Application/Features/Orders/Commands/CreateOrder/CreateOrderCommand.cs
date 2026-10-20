using MediatR;
using OrderApi.Application.Features.Orders.DTOs;

namespace OrderApi.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    IReadOnlyList<CreateOrderItemRequest> Items,
    string Currency = "USD",
    string? Notes = null) : IRequest<OrderDto>;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);