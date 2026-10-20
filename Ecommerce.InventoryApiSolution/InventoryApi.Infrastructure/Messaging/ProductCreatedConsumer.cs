using Ecommerce.SharedLibrary.Events;
using InventoryApi.Application.Abstractions;
using InventoryApi.Application.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InventoryApi.Infrastructure.Messaging;

public sealed class ProductCreatedConsumer(
    IInventoryService inventoryService,
    ILogger<ProductCreatedConsumer> logger)
    : IConsumer<ProductCreatedEvent>
{
    public async Task Consume(
        ConsumeContext<ProductCreatedEvent> context)
    {
        var message = context.Message;

        if (message.ProductId == Guid.Empty)
            throw new ArgumentException(
                "ProductCreatedEvent must contain a valid ProductId.");

        logger.LogInformation(
            "Received ProductCreatedEvent for product {ProductId}.",
            message.ProductId);

        var request = new CreateInventoryRequest(
            ProductId: message.ProductId,
            Sku: message.Sku,
            ProductName: message.ProductName,
            InitialQuantity: 0,
            ReorderLevel: 0,
            WarehouseCode: "MAIN");

        var inventory = await inventoryService.CreateFromProductAsync(
            request,
            context.CancellationToken);

        logger.LogInformation(
            "Inventory initialized for product {ProductId}. " +
            "InventoryId: {InventoryId}; available quantity: {Quantity}.",
            message.ProductId,
            inventory.Id,
            inventory.AvailableQuantity);
    }
}