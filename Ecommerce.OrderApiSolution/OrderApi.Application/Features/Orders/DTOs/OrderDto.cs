using OrderApi.Domain.Enums;

namespace OrderApi.Application.Features.Orders.DTOs;

public sealed record OrderDto(
    Guid Id, 
    Guid CustomerId, 
    string Currency, 
    OrderStatus Status, 
    decimal Subtotal, 
    decimal Total, 
    DateTimeOffset CreatedAt, 
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
    );