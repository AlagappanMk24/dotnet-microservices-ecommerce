using System.Security.Claims;
using Ecommerce.SharedLibrary.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderApi.Application.Common.Interfaces;
using OrderApi.Application.Features.Orders.DTOs;
using OrderApi.Domain.Entities;

namespace OrderApi.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(IOrderRepository orders, IProductCatalogClient catalog, 
    IOrderEventPublisher events, IHttpContextAccessor httpContextAccessor, ILogger<CreateOrderCommandHandler> logger) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var principal = httpContextAccessor.HttpContext?.User
                   ?? throw new UnauthorizedAccessException("Authenticated user is required.");

        var rawId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

        if (!Guid.TryParse(rawId, out var customerId))
            throw new UnauthorizedAccessException("JWT must contain a GUID subject or NameIdentifier claim.");

        var drafts = new List<OrderItemDraft>();

        foreach (var requested in request.Items)
        {
            var product = await catalog.GetProductAsync(requested.ProductId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException($"Product '{requested.ProductId}' was not found.");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product '{product.Id}' is inactive.");

            if (product.StockQuantity < requested.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product '{product.Id}'.");

            if (!string.Equals(product.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Product '{product.Id}' uses currency '{product.Currency}', not '{request.Currency}'.");

            drafts.Add(new OrderItemDraft(product.Id, product.Name, product.Sku, requested.Quantity, product.Price));
        }
        var order = CustomerOrder.Create(customerId, request.Currency, drafts, request.Notes);

        await orders.AddAsync(order, cancellationToken);

        await orders.SaveChangesAsync(cancellationToken);

        var placed = new OrderPlacedEvent(
                  order.Id,
                  order.CustomerId,
                  order.Total,
                  order.Items.Select(i => new OrderPlacedItem(i.ProductId, i.Quantity, i.UnitPrice)).ToArray());

        await events.PublishAsync(placed, cancellationToken);

        return Map(order);
    }

    private static OrderDto Map(CustomerOrder order) => new(
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
                i.LineTotal)).ToArray());
}