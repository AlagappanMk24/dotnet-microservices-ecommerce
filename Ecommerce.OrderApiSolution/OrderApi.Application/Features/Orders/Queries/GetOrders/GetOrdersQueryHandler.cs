using System.Security.Claims;
using Ecommerce.SharedLibrary.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using OrderApi.Application.Common.Interfaces;
using OrderApi.Application.Features.Orders.DTOs;

namespace OrderApi.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler(IOrderRepository orders, IHttpContextAccessor accessor)
    : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var principal = accessor.HttpContext?.User
             ?? throw new UnauthorizedAccessException("Authenticated user is required.");

        var rawId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (!Guid.TryParse(rawId, out var customerId))
            throw new UnauthorizedAccessException("JWT must contain a GUID subject or NameIdentifier claim.");

        var pagedOrders = await orders.GetAllForCustomerAsync(
            customerId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var orderDtos = pagedOrders.Items.Select(order => new OrderDto(
            order.Id,
            order.CustomerId,
            order.Currency,
            order.Status,
            order.Subtotal,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Sku,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal)).ToArray()
        )).ToArray();

        return new PagedResult<OrderDto>(
            orderDtos,
            pagedOrders.TotalCount,
            pagedOrders.PageNumber,
            pagedOrders.PageSize);
    }
}